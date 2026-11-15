import { $, api, esc, layout, money, stars } from "../app.js";

layout("hotels");
$("#main").innerHTML = `
  <section class="hero">
    <img src="/hero.jpg" alt="Hotel terrace at sunset" />
    <div class="wrap">
      <h1>Find your next stay.</h1>
      <input id="q" class="field" placeholder="Search by city or hotel…" />
    </div>
  </section>
  <div class="wrap"><div id="list" class="grid hotels"></div></div>`;

const hotels = await api("/hotels");

function render() {
  const q = $("#q").value.toLowerCase();
  const items = hotels.filter(({ hotel: h }) => (h.city + h.name).toLowerCase().includes(q));
  $("#list").innerHTML = items.length
    ? items.map(({ hotel: h, fromPrice }) => `
        <article class="card hotel-card">
          <img src="${esc(h.image)}" alt="${esc(h.name)}" loading="lazy" />
          <div class="body">
            <div class="row"><h3>${esc(h.name)}</h3>${stars(h.stars)}</div>
            <p class="muted">${esc(h.city)}</p>
            <p style="font-size:14px;margin:0">${esc(h.description)}</p>
            <div class="bottom">
              ${fromPrice != null ? `<span style="font-size:14px">from <b>${money(fromPrice)}</b>/night</span>` : `<span class="muted">No rooms yet</span>`}
              <a class="btn" href="/hotel.html?id=${h.id}">View details</a>
            </div>
          </div>
        </article>`).join("")
    : `<p class="muted">No hotels match.</p>`;
}
$("#q").addEventListener("input", render);
render();
