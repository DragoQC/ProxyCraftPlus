using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using ProxyCraftPlus.Api.Contracts;

namespace ProxyCraftPlus.Services;

public sealed class RconClient
{
    private const int MaximumPacketLength = 65536;
    private const int MaximumOutputLength = 262144;

    public async Task CheckAsync(string host, int port, string password, CancellationToken cancellationToken)
    {
        using TcpClient connection = new() { NoDelay = true };
        await connection.ConnectAsync(host, port, cancellationToken);
        await AuthenticateAsync(connection.GetStream(), password, cancellationToken);
    }

    public async Task<RconCommandResponse> ExecuteAsync(string host, int port, string password, string command, CancellationToken cancellationToken)
    {
        using TcpClient connection = new() { NoDelay = true };
        await connection.ConnectAsync(host, port, cancellationToken);
        NetworkStream stream = connection.GetStream();
        await AuthenticateAsync(stream, password, cancellationToken);
        await WritePacketAsync(stream, 2, 2, command, cancellationToken);
        RconPacket packet = await ReadPacketAsync(stream, cancellationToken);

        // A distinct, empty RESPONSE_VALUE request marks the end of this response.
        // Minecraft replies to this unsupported request type without executing a command.
        // Send it after the first response so older servers cannot coalesce/drop it with the command.
        await WritePacketAsync(stream, 3, 0, string.Empty, cancellationToken);
        using MemoryStream output = new();
        for (int count = 0; count < 1024; count++)
        {
            if (packet.Type != 0)
            {
                throw new RconException("The server returned an invalid RCON response.");
            }
            if (packet.Id == 3)
            {
                return new RconCommandResponse(Encoding.UTF8.GetString(output.ToArray()), false, DateTimeOffset.UtcNow);
            }
            if (packet.Id != 2)
            {
                throw new RconException("The server returned an unexpected RCON response ID.");
            }

            int remaining = MaximumOutputLength - (int)output.Length;
            output.Write(packet.Payload.AsSpan(0, Math.Min(remaining, packet.Payload.Length)));
            if (packet.Payload.Length > remaining)
            {
                return new RconCommandResponse(Encoding.UTF8.GetString(output.ToArray()), true, DateTimeOffset.UtcNow);
            }
            packet = await ReadPacketAsync(stream, cancellationToken);
        }
        throw new RconException("The server sent too many RCON response packets.");
    }

    private static async Task AuthenticateAsync(NetworkStream stream, string password, CancellationToken cancellationToken)
    {
        await WritePacketAsync(stream, 1, 3, password, cancellationToken);
        for (int count = 0; count < 4; count++)
        {
            RconPacket packet = await ReadPacketAsync(stream, cancellationToken);
            if (packet.Id == -1)
            {
                throw new RconException("The RCON password was rejected.");
            }
            if (packet.Id != 1)
            {
                throw new RconException("The server returned an unexpected RCON authentication ID.");
            }
            if (packet.Type == 2)
            {
                return;
            }
            if (packet.Type != 0 || packet.Payload.Length != 0)
            {
                break;
            }
        }
        throw new RconException("The server returned an invalid RCON authentication response.");
    }

    private static async Task WritePacketAsync(NetworkStream stream, int id, int type, string text, CancellationToken cancellationToken)
    {
        byte[] payload = Encoding.UTF8.GetBytes(text);
        byte[] packet = new byte[payload.Length + 14];
        BinaryPrimitives.WriteInt32LittleEndian(packet, payload.Length + 10);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(4), id);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(8), type);
        payload.CopyTo(packet, 12);
        await stream.WriteAsync(packet, cancellationToken);
    }

    private static async Task<RconPacket> ReadPacketAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        byte[] header = new byte[4];
        await stream.ReadExactlyAsync(header, cancellationToken);
        int length = BinaryPrimitives.ReadInt32LittleEndian(header);
        if (length is < 10 or > MaximumPacketLength)
        {
            throw new RconException("The server returned an invalid RCON packet size.");
        }
        byte[] packet = new byte[length];
        await stream.ReadExactlyAsync(packet, cancellationToken);
        if (packet[^1] != 0 || packet[^2] != 0)
        {
            throw new RconException("The server returned an invalid RCON packet terminator.");
        }
        return new RconPacket(BinaryPrimitives.ReadInt32LittleEndian(packet),
            BinaryPrimitives.ReadInt32LittleEndian(packet.AsSpan(4)), packet[8..^2]);
    }

    private sealed record RconPacket(int Id, int Type, byte[] Payload);
}
