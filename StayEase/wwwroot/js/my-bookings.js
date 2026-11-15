import { $, api, badge, esc, layout, money, run } from "../app.js";

layout("bookings");
$("#main").innerHTML = `<div class="wrap page"><h1 style="font-size:36px">My bookings</h1><div id="out"></div></div>`;

async function render() {
  const mine = await api("/bookings?customerId=1");
  $("#out").innerHTML = mine.length ? `
    <div class="table-wrap"><table>
      <thead><tr>${["Hotel", "Room", "Check-in", "Check-out", "Nights", "Total", "Status", ""].map((h) => `<th>${h}</th>`).join("")}</tr></thead>
      <tbody>${mine.map((b) => `
        <tr>
          <td>${esc(b.hotel)}</td><td>${esc(b.room)}</td><td>${esc(b.checkIn)}</td><td>${esc(b.checkOut)}</td>
          <td>${b.nights}</td><td>${money(b.total)}</td><td>${badge(b.status)}</td>
          <td>${b.status !== "Cancelled" ? `<button class="btn-link" data-cancel="${b.id}">Cancel</button>` : ""}</td>
        </tr>`).join("")}
      </tbody></table></div>`
    : `<p>No bookings yet. <a href="/" style="color:var(--primary)">Browse hotels</a></p>`;
}

$("#out").addEventListener("click", async (e) => {
  const id = e.target.dataset.cancel; if (!id) return;
  await run(() => api(`/bookings/${id}/status`, "PUT", { status: "Cancelled" }), "Booking cancelled");
  render();
});
render();
