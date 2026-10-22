export function initializeRconConsole({ api, path, getPassword, canSend, showError }) {
    const form = document.querySelector("#rcon-command");
    const field = form.elements.command;
    const send = form.querySelector('button[type="submit"]');
    const output = document.querySelector("#console-output");
    const error = document.querySelector("#console-error");
    let pending = false;
    let request;

    function refresh() {
        const ready = canSend();
        send.disabled = pending || !ready;
        field.disabled = pending || !ready;
        document.querySelector("#console-state").textContent = ready
            ? "Enter a Minecraft command and view its response."
            : "Enable RCON, save the connection, and enter its password to use the console.";
    }

    function append(text) {
        output.textContent = (output.textContent + text).slice(-100000);
        output.scrollTop = output.scrollHeight;
    }

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        if (pending || !canSend()) return;
        error.classList.add("hidden");
        const command = field.value.trim();
        if (!command) {
            showError(new Error("Enter a command."), error);
            field.focus();
            return;
        }
        pending = true;
        request = new AbortController();
        refresh();
        append(`> ${command}\n`);
        try {
            const result = await api(`${path}/commands`, { method: "POST", body: JSON.stringify({ password: getPassword(), command }), signal: request.signal });
            append((result.output || "(No output)") + (result.truncated ? "\n[Output truncated]" : "") + "\n");
            field.value = "";
        } catch (failure) {
            if (failure.name !== "AbortError") {
                showError(failure, error);
                append("[Request failed. Check the error below before retrying.]\n");
            }
        } finally {
            pending = false;
            request = null;
            refresh();
            if (!field.disabled) field.focus();
        }
    });
    document.querySelector("#clear-console").addEventListener("click", () => {
        output.textContent = "";
        error.classList.add("hidden");
    });
    window.addEventListener("pagehide", () => request?.abort());
    refresh();
    return { refresh };
}
