export async function initializeRconSettings(api, handleForm) {
    const form = document.querySelector("#rcon-settings");
    const id = Number(form.dataset.proxyId);
    const route = (await api("/proxies")).find((proxy) => proxy.id === id);
    if (!route) throw new Error("This proxy no longer exists. Return to configured proxies.");
    const enabled = form.elements.enabled;
    const fields = document.querySelector("#rcon-backend-fields");
    const port = form.elements.destinationPort;
    document.querySelector("#rcon-route-domain").textContent = route.domain;
    document.querySelector("#rcon-backend-host").textContent = route.destinationHost;
    enabled.checked = route.rconEnabled;
    enabled.disabled = false;
    port.value = route.rconDestinationPort;
    function updateFields() {
        fields.classList.toggle("hidden", !enabled.checked);
        fields.disabled = !enabled.checked;
        document.querySelector("#rcon-settings-status").textContent = "";
    }
    enabled.addEventListener("change", updateFields);
    updateFields();
    handleForm("#rcon-settings", async () => {
        if (!port.value || !Number.isInteger(Number(port.value)) || Number(port.value) < 1 || Number(port.value) > 65535) {
            throw new Error("Enter a backend RCON port between 1 and 65535.");
        }
        const controls = [...form.querySelectorAll("input")];
        controls.forEach((control) => { control.disabled = true; });
        try {
            await api(`/proxies/${id}/rcon`, { method: "PUT", body: JSON.stringify({ enabled: enabled.checked, destinationPort: Number(port.value) }) });
            document.querySelector("#rcon-settings-status").textContent = "RCON settings saved. Minecraft connections were kept open.";
        } finally {
            enabled.disabled = false;
            port.disabled = false;
            fields.disabled = !enabled.checked;
        }
    }, document.querySelector("#rcon-settings-error"));
}
