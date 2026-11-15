import { $, api, badge, esc, layout, money, run } from "../app.js";

layout("admin");
$("#main").innerHTML = `
  <div class="wrap page">
    <h1 style="font-size:36px">Admin dashboard</h1>
    <div id="kpis" class="grid kpis"></div>

    <section class="form"><h2 style="font-size:24px">Hotels</h2>
      <form id="hotel-form" class="form f5">
        <input class="field" name="name" placeholder="Name" />
        <input class="field" name="city" placeholder="City" />
        <input class="field" name="stars" type="number" min="1" max="5" value="4" />
        <input class="field" name="image" placeholder="Image URL (optional)" />
        <button class="btn">Add hotel</button>
      </form>
      <div id="hotels"></div>
    </section>

    <section class="form"><h2 style="font-size:24px">Rooms</h2>
      <form id="room-form" class="form f6">
        <select class="field" name="hotelId" id="room-hotel"></select>
        <input class="field" name="number" placeholder="Number" />
        <select class="field" name="type"><option>Standard</option><option>Deluxe</option><option>Suite</option></select>
        <input class="field" name="price" type="number" min="1" value="100" />
        <input class="field" name="capacity" type="number" min="1" value="2" />
        <button class="btn">Add room</button>
      </form>
      <div id="rooms"></div>
    </section>

    <section class="form"><h2 style="font-size:24px">Bookings</h2><div id="bookings"></div></section>
  </div>`;

const table = (head, rows) => `
  <div class="table-wrap"><table>
    <thead><tr>${head.map((h) => `<th>${h}</th>`).join("")}</tr></thead>
    <tbody>${rows.map((r) => `<tr>${r.map((c) => `<td>${c}</td>`).join("")}</tr>`).join("")}</tbody>
  </table></div>`;

async function render() {
  const [stats, hotels, bookings] = await Promise.all([api("/stats"), api("/hotels"), api("/bookings")]);
  const details = await Promise.all(hotels.map((h) => api(`/hotels/${h.hotel.id}`)));
  const rooms = details.flatMap((d) => d.rooms.map((r) => ({ ...r, hotel: d.hotel.name })));

  $("#kpis").innerHTML = [["Revenue", money(stats.revenue)], ["Bookings", stats.bookings], ["Hotels", stats.hotels], ["Rooms", stats.rooms]]
    .map(([k, v]) => `<div class="card kpi"><span class="muted">${k}</span><b>${v}</b></div>`).join("");

  $("#hotels").innerHTML = table(["Name", "City", "Stars", ""],
    hotels.map(({ hotel: h }) => [esc(h.name), esc(h.city), h.stars, `<button class="btn-link" data-del-hotel="${h.id}">Delete</button>`]));

  $("#room-hotel").innerHTML = `<option value="0">Hotel…</option>` + hotels.map(({ hotel: h }) => `<option value="${h.id}">${esc(h.name)}</option>`).join("");

  $("#rooms").innerHTML = table(["Hotel", "Room", "Type", "Price", "Guests", ""],
    rooms.map((r) => [esc(r.hotel), esc(r.number), esc(r.type), money(r.price), r.capacity, `<button class="btn-link" data-del-room="${r.id}">Delete</button>`]));

  $("#bookings").innerHTML = table(["#", "Customer", "Dates", "Total", "Status", "Change"],
    bookings.map((b) => [b.id, b.customerId, `${esc(b.checkIn)} → ${esc(b.checkOut)}`, money(b.total), badge(b.status),
      `<select class="field" data-status="${b.id}">${["Pending", "Confirmed", "Cancelled"].map((s) => `<option ${s === b.status ? "selected" : ""}>${s}</option>`).join("")}</select>`]));
}

const val = (form) => Object.fromEntries(new FormData(form));

$("#hotel-form").addEventListener("submit", async (e) => {
  e.preventDefault(); const f = val(e.target);
  const ok = await run(() => api("/hotels", "POST", { name: f.name, city: f.city, stars: Number(f.stars), image: f.image || null, description: "" }), "Hotel added");
  if (ok) { e.target.reset(); render(); }
});
$("#room-form").addEventListener("submit", async (e) => {
  e.preventDefault(); const f = val(e.target);
  const ok = await run(() => api("/rooms", "POST", { hotelId: Number(f.hotelId), number: f.number, type: f.type, price: Number(f.price), capacity: Number(f.capacity) }), "Room added");
  if (ok !== undefined) render();
});
document.addEventListener("click", async (e) => {
  const { delHotel, delRoom } = e.target.dataset;
  if (delHotel) { await run(() => api(`/hotels/${delHotel}`, "DELETE"), "Hotel deleted"); render(); }
  if (delRoom) { await run(() => api(`/rooms/${delRoom}`, "DELETE"), "Room deleted"); render(); }
});
document.addEventListener("change", async (e) => {
  const id = e.target.dataset.status; if (!id) return;
  await run(() => api(`/bookings/${id}/status`, "PUT", { status: e.target.value }), "Status updated");
  render();
});
render();
