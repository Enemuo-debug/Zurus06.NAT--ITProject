import { redraw, getObjectAt, findDevice } from "./canvas.js";

window.addEventListener("DOMContentLoaded", () => {
  const canvas = document.querySelector(".SimulationArea");
  const ctx = canvas.getContext("2d");

  const joinBtn = document.getElementById("join");
  const saveBtn = document.getElementById("saveBtn");
  const deleteBtn = document.getElementById("deleteBtn");
  const backBtn = document.getElementById("backBtn");
  const clearLogBtn = document.getElementById("clearlog");
  const logOutput = document.getElementById("logOutput");
  const descriptionBox = document.querySelector(".Description p");

  const deviceList = { Router: [0], Server: [0], Switch: [0], PC: [0] };
  let devices = [];   
  let edges = [];
  let dragging = null;
  let offsetX = 0, offsetY = 0;
  let linkMode = false;
  let linkStart = null;

  const simulationId = new URLSearchParams(window.location.search).get("id");

  const deviceDescriptions = {
    PC: "A user-end device that connects to the network.",
    Switch: "Connects multiple devices within a LAN.",
    Router: "Routes data between networks.",
    Server: "Provides data or services to clients."
  };

  function addLog(msg, type = "info") {
    const entry = document.createElement("div");
    entry.className = `log ${type}`;
    entry.textContent = `> ${msg}`;
    logOutput.appendChild(entry);
    logOutput.scrollTop = logOutput.scrollHeight;
  }

  function fixCanvasResolution() {
    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width;
    canvas.height = rect.height;
    redraw(ctx, canvas, devices, edges);
  }
  fixCanvasResolution();
  window.addEventListener("resize", fixCanvasResolution);

  // wire device palette clicks and drags
  document.querySelectorAll(".device").forEach(node => {
    node.addEventListener("click", () => {
      const t = node.dataset.device;
      descriptionBox.innerHTML = deviceDescriptions[t] || "No description";
    });

    node.addEventListener("dragstart", e => {
      if (linkMode) return;
      e.dataTransfer.setData("device", node.dataset.device);
    });
  });

  // drop onto canvas
  canvas.addEventListener("dragover", e => e.preventDefault());
  canvas.addEventListener("drop", e => {
    if (linkMode) return;
    e.preventDefault();

    const type = e.dataTransfer.getData("device");
    if (!type) return;

    const rect = canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    // compute next id for that type
    if (!deviceList[type]) deviceList[type] = [0];
    const ids = deviceList[type];
    const nextId = (ids.length ? ids[ids.length - 1] : 0) + 1;
    ids.push(nextId);

    const device = {
      Type: type,
      Id: nextId,
      X: x - 20,
      Y: y - 20,
      Width: 40,
      Height: 40
    };

    devices.push(device);

    addLog(`${type} ${nextId} added`, "success");
    redraw(ctx, canvas, devices, edges);
  });

  // mouse interactions: move, delete, link
  canvas.addEventListener("mousedown", e => {
    e.preventDefault();
    const rect = canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const obj = getObjectAt(devices, x, y);

    // not in link mode -> move or delete on right click
    if (!linkMode) {
      if (e.button === 0 && obj) {
        dragging = obj;
        offsetX = x - obj.X;
        offsetY = y - obj.Y;
      } else if (e.button === 2 && obj) {
        // delete device and all edges referencing it
        devices = devices.filter(d => !(d.Type === obj.Type && d.Id === obj.Id));

        edges = edges.filter(edge =>
          !(edge.from.Type === obj.Type && edge.from.Id === obj.Id) &&
          !(edge.to.Type === obj.Type && edge.to.Id === obj.Id)
        );

        addLog(`${obj.Type} ${obj.Id} deleted`, "error");
        redraw(ctx, canvas, devices, edges);
      }
      return;
    }

    // link mode
    if (linkMode) {
      if (!obj) return;

      if (!linkStart) {
        linkStart = obj;
        addLog(`Selected ${obj.Type} ${obj.Id} as link start`, "info");
        return;
      }

      // clicked target
      const target = obj;

      // same device => cancel
      if (linkStart.Type === target.Type && linkStart.Id === target.Id) {
        addLog("Cannot link device to itself", "error");
        linkStart = null;
        return;
      }

      // check existing (bidirectional check)
      const exists = edges.some(e =>
        (e.from.Type === linkStart.Type && e.from.Id === linkStart.Id && e.to.Type === target.Type && e.to.Id === target.Id) ||
        (e.to.Type === linkStart.Type && e.to.Id === linkStart.Id && e.from.Type === target.Type && e.from.Id === target.Id)
      );

      if (!exists) {
        edges.push({
          from: { Type: linkStart.Type, Id: linkStart.Id },
          to:   { Type: target.Type, Id: target.Id }
        });
        addLog(`Linked ${linkStart.Type} ${linkStart.Id} → ${target.Type} ${target.Id}`, "success");
        redraw(ctx, canvas, devices, edges);
      } else {
        addLog("Link already exists", "info");
      }

      linkStart = null;
    }
  });

  canvas.addEventListener("mousemove", e => {
    if (!dragging) return;
    const rect = canvas.getBoundingClientRect();
    dragging.X = e.clientX - rect.left - offsetX;
    dragging.Y = e.clientY - rect.top - offsetY;
    redraw(ctx, canvas, devices, edges);
  });

  canvas.addEventListener("mouseup", () => dragging = null);
  canvas.addEventListener("contextmenu", e => e.preventDefault());

  if (clearLogBtn) clearLogBtn.addEventListener("click", () => {
    logOutput.replaceChildren();
    addLog("Console cleared");
  });

  joinBtn.addEventListener("click", () => {
    linkMode = !linkMode;
    joinBtn.classList.toggle("active");
    linkStart = null;
    addLog(linkMode ? "Link mode ON" : "Link mode OFF");
  });

  function rebuildDeviceListFromDevices() {
    for (const k of Object.keys(deviceList)) {
      deviceList[k] = [0];
    }
    devices.forEach(d => {
      if (!deviceList[d.Type]) deviceList[d.Type] = [0];
      deviceList[d.Type].push(d.Id);
    });
  }

  async function loadSimulation() {
    if (!simulationId) {
      addLog("No simulation ID provided in URL", "info");
      return;
    }

    try {
      const res = await fetch(`http://localhost:5245/diagrams/${simulationId}`, {
        credentials: "include"
      });

      if (!res.ok) {
        const text = await res.text().catch(() => "");
        throw new Error(text || `HTTP ${res.status}`);
      }

      const body = await res.json();
      const sim = body && body.data;
      if (!sim) {
        addLog("Simulation not found", "error");
        return;
      }

      if (!sim.dataJson) {
        addLog("Simulation has no saved data", "info");
        return;
      }

      const parsed = JSON.parse(sim.dataJson);
      devices = parsed.devices || [];
      edges = parsed.links || [];

      devices = devices.map(d => ({
        Type: d.Type,
        Id: typeof d.Id === "string" ? parseInt(d.Id, 10) : d.Id,
        X: Number(d.X),
        Y: Number(d.Y),
        Width: Number(d.Width || 40),
        Height: Number(d.Height || 40)
      }));

      edges = edges.map(e => ({
        from: { Type: e.from.Type, Id: typeof e.from.Id === "string" ? parseInt(e.from.Id, 10) : e.from.Id },
        to:   { Type: e.to.Type,   Id: typeof e.to.Id === "string"   ? parseInt(e.to.Id, 10)   : e.to.Id }
      }));

      rebuildDeviceListFromDevices();
      redraw(ctx, canvas, devices, edges);
      addLog("Simulation loaded successfully", "success");
    } catch (err) {
      addLog("Could not load simulation: " + (err.message || err), "error");
    }
  }
  loadSimulation();

  // ========== SAVE SIMULATION ==========
  saveBtn.addEventListener("click", async () => {
    if (!simulationId) return addLog("Missing simulation ID", "error");

    const payload = {
      dataJson: JSON.stringify({
        devices: devices,
        links: edges
      })
    };

    try {
      const res = await fetch(`http://localhost:5245/diagrams/${simulationId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify(payload)
      });

      if (!res.ok) {
        const txt = await res.text().catch(() => "");
        throw new Error(txt || `HTTP ${res.status}`);
      }

      addLog("Simulation saved successfully", "success");
    } catch (err) {
      addLog("Error saving simulation: " + (err.message || err), "error");
    }
  });

  // ========== DELETE SIMULATION ==========
  if (deleteBtn) {
    deleteBtn.addEventListener("click", async () => {
      if (!simulationId) return addLog("Missing simulation ID", "error");
      if (!confirm("Are you sure you want to delete this simulation?")) return;

      try {
        const res = await fetch(`http://localhost:5245/diagrams/${simulationId}`, {
          method: "DELETE",
          credentials: "include"
        });

        if (!res.ok) {
          const txt = await res.text().catch(() => "");
          throw new Error(txt || `HTTP ${res.status}`);
        }

        addLog("Simulation deleted", "success");
        window.location.href = "simulations.html";
      } catch (err) {
        addLog("Error deleting simulation: " + (err.message || err), "error");
      }
    });
  }

  // optional back button
  if (backBtn) backBtn.addEventListener("click", () => {
    window.location.href = "simulations.html";
  });
});