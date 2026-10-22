import { initializePasswordInput } from "./components/password-input.js";
import { initializeRconConsole } from "./components/rcon-console.js";

const passwordPrefix = "proxycraftplus.rcon.password:";

export function forgetRconPasswords(id) {
    try {
        for (const key of Object.keys(localStorage)) {
            if (!key.startsWith(passwordPrefix)) continue;
            let identity;
            try {
                identity = JSON.parse(key.slice(passwordPrefix.length));
            } catch {
                continue;
            }
            if (Array.isArray(identity) && identity[0] === id) localStorage.removeItem(key);
        }
    } catch {
        // Unavailable browser storage does not undo an already committed route deletion.
    }
}

export async function initializeRconSettings(api, handleForm, showError) {
    const form = document.querySelector("#rcon-settings");
    const id = Number(form.dataset.proxyId);
    let route = (await api("/proxies")).find((proxy) => proxy.id === id);
    if (!route) throw new Error("This proxy no longer exists. Return to configured proxies.");
    const enabled = form.elements.enabled;
    const fields = document.querySelector("#rcon-backend-fields");
    const port = form.elements.destinationPort;
    const password = form.elements.password;
    const status = document.querySelector("#rcon-settings-status");
    const checkStatus = document.querySelector("#rcon-check-status");
    const check = document.querySelector("#rcon-check");
    const error = document.querySelector("#rcon-settings-error");
    const path = `/proxies/${id}/rcon`;
    let checking = false;
    let saving = false;
    let checkRequest;
    const storageKey = (value) => `${passwordPrefix}${JSON.stringify([value.id, value.destinationHost, value.rconDestinationPort])}`;
    let key = storageKey(route);

    document.querySelector("#rcon-route-domain").textContent = route.domain;
    document.querySelector("#rcon-backend-host").textContent = route.destinationHost;
    enabled.checked = route.rconEnabled;
    port.value = route.rconDestinationPort;
    try {
        password.value = localStorage.getItem(key) || "";
    } catch {
        status.textContent = "Browser storage is unavailable. Your password will stay on this page only.";
    }
    initializePasswordInput(password, document.querySelector("#toggle-rcon-password"));
    const consoleView = initializeRconConsole({ api, path, getPassword: () => password.value,
        canSend: () => route.rconEnabled && route.isActive && enabled.checked === route.rconEnabled
            && Number(port.value) === route.rconDestinationPort && password.value.length > 0 && !saving,
        showError });

    function refresh() {
        enabled.disabled = saving || checking;
        fields.classList.toggle("hidden", !enabled.checked);
        fields.disabled = saving || checking || !enabled.checked;
        check.disabled = checking || saving || !route.rconEnabled || !route.isActive || !password.value;
        form.querySelector('button[type="submit"]').disabled = saving || checking;
        consoleView.refresh();
    }
    form.addEventListener("input", () => {
        status.textContent = "";
        checkStatus.textContent = "Not checked with these settings.";
        error.classList.add("hidden");
        refresh();
    });
    enabled.addEventListener("change", refresh);

    handleForm("#rcon-settings", async () => {
        if (!port.value || !Number.isInteger(Number(port.value)) || Number(port.value) < 1 || Number(port.value) > 65535) {
            throw new Error("Enter an RCON port between 1 and 65535.");
        }
        saving = true;
        refresh();
        try {
            route = await api(path, { method: "PUT", body: JSON.stringify({ enabled: enabled.checked, destinationPort: Number(port.value) }) });
            const previousKey = key;
            key = storageKey(route);
            status.textContent = "Connection saved. Minecraft connections were kept open.";
            try {
                if (previousKey !== key) localStorage.removeItem(previousKey);
                if (password.value) localStorage.setItem(key, password.value);
                else localStorage.removeItem(key);
            } catch {
                status.textContent = "Connection saved. Browser storage is unavailable; the password stays on this page only.";
            }
        } finally {
            saving = false;
            refresh();
        }
    }, error);
    check.addEventListener("click", async () => {
        if (checking) return;
        error.classList.add("hidden");
        if (enabled.checked !== route.rconEnabled || Number(port.value) !== route.rconDestinationPort) {
            showError(new Error("Save the connection settings before testing."), error);
            return;
        }
        checking = true;
        checkRequest = new AbortController();
        refresh();
        checkStatus.textContent = "Checking RCON authentication…";
        try {
            const result = await api(`${path}/check`, { method: "POST", body: JSON.stringify({ password: password.value }), signal: checkRequest.signal });
            checkStatus.textContent = `Connected. RCON authenticated at ${new Date(result.checkedAt).toLocaleTimeString()}.`;
        } catch (failure) {
            if (failure.name !== "AbortError") {
                checkStatus.textContent = "Connection unavailable.";
                showError(failure, error);
            }
        } finally {
            checking = false;
            checkRequest = null;
            refresh();
        }
    });
    window.addEventListener("pagehide", () => checkRequest?.abort());
    refresh();
}
