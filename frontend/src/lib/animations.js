import gsap from "gsap";
function prefersReducedMotion() {
    if (typeof window === "undefined")
        return false;
    return window.matchMedia("(prefers-reduced-motion: reduce)").matches;
}
export function animateWorkspaceTransition(container) {
    if (!container || prefersReducedMotion())
        return;
    gsap.killTweensOf(container);
    const tl = gsap.timeline({ defaults: { ease: "power3.inOut" } });
    tl.fromTo(container, { opacity: 0.55, scale: 0.99, y: 6 }, { opacity: 1, scale: 1, y: 0, duration: 0.26, clearProps: "transform,opacity" });
}
export function scrollToAndPulseHighlight(targetElementId) {
    if (typeof document === "undefined")
        return;
    const targetEl = document.getElementById(targetElementId);
    if (!targetEl)
        return;
    targetEl.scrollIntoView({
        behavior: prefersReducedMotion() ? "auto" : "smooth",
        block: "center"
    });
    if (prefersReducedMotion())
        return;
    gsap.fromTo(targetEl, {
        boxShadow: "0 0 0 8px rgba(99, 102, 241, 0.45)",
        scale: 1.015
    }, {
        boxShadow: "0 0 0 0px rgba(99, 102, 241, 0)",
        scale: 1,
        duration: 1.15,
        ease: "power2.out",
        clearProps: "scale,boxShadow"
    });
}
export async function waitForElementAndPulse(targetElementId, maxWaitMs = 2500, pollIntervalMs = 50) {
    if (typeof document === "undefined")
        return false;
    const startTime = Date.now();
    while (Date.now() - startTime < maxWaitMs) {
        const el = document.getElementById(targetElementId);
        if (el) {
            scrollToAndPulseHighlight(targetElementId);
            return true;
        }
        await new Promise((resolve) => setTimeout(resolve, pollIntervalMs));
    }
    return false;
}
export function triggerCompletionCelebration(containerId) {
    if (typeof document === "undefined" || prefersReducedMotion())
        return;
    const container = document.getElementById(containerId);
    if (!container)
        return;
    const colors = ["#6366F1", "#10B981", "#F59E0B", "#EC4899", "#3B82F6"];
    for (let i = 0; i < 45; i++) {
        const particle = document.createElement("div");
        particle.className = "absolute rounded-sm pointer-events-none z-50";
        const size = gsap.utils.random(6, 12);
        particle.style.width = `${size}px`;
        particle.style.height = `${size * 0.65}px`;
        particle.style.backgroundColor = gsap.utils.random(colors);
        particle.style.left = "50%";
        particle.style.top = "45%";
        container.appendChild(particle);
        const angle = Math.random() * Math.PI * 2;
        const velocity = gsap.utils.random(120, 340);
        gsap.to(particle, {
            x: Math.cos(angle) * velocity,
            y: Math.sin(angle) * velocity + gsap.utils.random(60, 160),
            rotation: gsap.utils.random(0, 540),
            opacity: 0,
            duration: gsap.utils.random(1.0, 1.8),
            ease: "power3.out",
            onComplete: () => particle.remove()
        });
    }
}

export default {
    animateWorkspaceTransition,
    scrollToAndPulseHighlight,
    waitForElementAndPulse,
    triggerCompletionCelebration
};
