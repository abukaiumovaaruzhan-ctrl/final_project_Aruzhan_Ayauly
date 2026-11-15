// Общий код для всех страниц: запросы к C#-API, шапка/подвал, уведомления.

export const $ = (sel, root = document) => root.querySelector(sel);

// Защита от XSS: всё, что пришло с сервера, экранируем перед вставкой в HTML.
export const esc = (v) =>
  String(v ?? "").replace(/[&<>"']/g, (c) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

export const money = (n) => "$" + Number(n).toLocaleString("en-US");
export const stars = (n) => `<span class="stars">${"★".repeat(n)}</span>`;
export const badge = (s) => `<span class="badge ${esc(s)}">${esc(s)}</span>`;

export function toast(message, ok = true) {
  let box = $("#toast");
  if (!box) { box = document.createElement("div"); box.id = "toast"; document.body.append(box); }
  const el = document.createElement("div");
  el.className = ok ? "ok" : "err";
  el.textContent = message;
  box.append(el);
  setTimeout(() => el.remove(), 3500);
}

export async function api(path, method = "GET", body) {
  const res = await fetch("/api" + path, {
    method,
    headers: body ? { "Content-Type": "application/json" } : undefined,
    body: body ? JSON.stringify(body) : undefined,
  });
  if (res.status === 204) return null;
  const data = await res.json().catch(() => ({}));
  if (!res.ok) throw new Error(data.error ?? `Request failed (${res.status})`);
  return data;
}

// Выполнить действие и показать уведомление об успехе/ошибке.
export async function run(action, okMessage) {
  try { const r = await action(); if (okMessage) toast(okMessage); return r; }
  catch (e) { toast(e.message, false); }
}

export function layout(active) {
  const link = (href, name, text) =>
    `<a href="${href}" class="${active === name ? "active" : ""}">${text}</a>`;
  $("#app-header").innerHTML = `
    <div class="wrap">
      <a href="/" class="logo">Stay<span>Ease</span></a>
      <nav>${link("/", "hotels", "Hotels")}${link("/my-bookings.html", "bookings", "My bookings")}${link("/admin.html", "admin", "Admin")}</nav>
    </div>`;
  $("#app-footer").textContent = "StayEase · Hotel Booking System";
}
