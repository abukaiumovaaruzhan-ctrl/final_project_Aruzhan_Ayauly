import { $, api, esc, layout, money, run, stars } from "../app.js";

layout("hotels");
const id = new URLSearchParams(location.search).get("id");

let data;
try { data = await api(`/hotels/${encodeURIComponent(id)}`); }
catch { $("#main").innerHTML = `<p style="padding:40px;text-align:center">Hotel not found. <a href="/" style="color:var(--primary)">Back</a></p>`; throw 0; }

const { hotel, rooms } = data;
document.title = `${hotel.name} — StayEase`;
let roomId = null;

$("#main").innerHTML = `
  <img class="details-img" src="${esc(hotel.image)}" alt="${esc(hotel.name)}" />
  <div class="wrap details">
    <div>
      <h1 style="font-size:36px">${esc(hotel.name)}</h1>
      <p class="muted" style="margin-top:4px">${esc(hotel.city)} · ${stars(hotel.stars)}</p>
      <p style="margin-top:16px">${esc(hotel.description)}</p>
      <h2 style="font-size:24px;margin:32px 0 16px">Rooms</h2>
      <div class="rooms" id="rooms"></div>
    </div>
    <aside class="card book">
      <h3 style="font-size:20px">Book your stay</h3>
      <label style="font-size:14px">Check-in<input id="in" type="date" class="field" /></label>
      <label style="font-size:14px">Check-out<input id="out" type="date" class="field" /></label>
      <p id="summary" style="font-size:14px;margin:0">Select a room</p>
      <p id="discount" style="font-size:12px;color:var(--success);margin:0" hidden>10% weekly discount applied</p>
      <p id="total" class="total" style="margin:0">—</p>
      <button id="confirm" class="btn">Confirm booking</button>
    </aside>
  </div>`;

function renderRooms() {
  $("#rooms").innerHTML = rooms.length
    ? rooms.map((r) => `
        <button class="room ${r.id === roomId ? "selected" : ""}" data-id="${r.id}">
          <p style="margin:0;font-weight:600">${esc(r.type)} · Room ${esc(r.number)}</p>
          <p class="muted" style="margin:4px 0 0">Up to ${r.capacity} guests</p>
          <p style="margin:8px 0 0"><b>${money(r.price)}</b>/night</p>
        </button>`).join("")
    : `<p class="muted">No rooms yet.</p>`;
}
renderRooms();
$("#rooms").addEventListener("click", (e) => {
  const b = e.target.closest(".room"); if (!b) return;
  roomId = Number(b.dataset.id); renderRooms(); update();
});

// Те же правила, что в Pricing.CalculatePrice на сервере (сервер всё равно перепроверяет).
function update() {
  const room = rooms.find((r) => r.id === roomId);
  const nights = $("#in").value && $("#out").value
    ? Math.round((new Date($("#out").value) - new Date($("#in").value)) / 86400000) : 0;
  $("#summary").textContent = room ? `${room.type} · ${Math.max(nights, 0)} nights` : "Select a room";
  $("#discount").hidden = nights < 7;
  let total = "—";
  if (room && nights > 0) total = money(Math.round(room.price * nights * (nights >= 7 ? 0.9 : 1) * 100) / 100);
  $("#total").textContent = total;
}
$("#in").addEventListener("input", update);
$("#out").addEventListener("input", update);

$("#confirm").addEventListener("click", async () => {
  if (!roomId) return run(() => { throw new Error("Choose a room."); });
  if (!$("#in").value || !$("#out").value) return run(() => { throw new Error("Pick check-in and check-out dates."); });
  const ok = await run(() => api("/bookings", "POST", { roomId, checkIn: $("#in").value, checkOut: $("#out").value }), "Booking created!");
  if (ok) setTimeout(() => (location.href = "/my-bookings.html"), 600);
});
