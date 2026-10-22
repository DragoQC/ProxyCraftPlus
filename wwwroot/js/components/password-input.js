export function initializePasswordInput(input, button) {
    button.addEventListener("click", () => {
        const visible = input.type === "password";
        input.type = visible ? "text" : "password";
        const label = `${visible ? "Hide" : "Show"} RCON password`;
        button.setAttribute("aria-label", label);
        button.setAttribute("title", label);
        button.setAttribute("aria-pressed", String(visible));
        button.querySelector("[data-password-hidden]").classList.toggle("hidden", !visible);
    });
}
