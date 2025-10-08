const devices = document.querySelectorAll(".device");
const deviceList = { Router: [0], Server: [0], Switch: [0], PC: [0] };
let edgeList = [];
let link = false;
let start = null;
const deviceDescriptions = {
    PC: `
    <strong>PC (Personal Computer):</strong> A user-end device that connects to the network 
    through a switch or router. It sends and receives packets, runs applications, 
    and serves as an endpoint in your simulation.
    `,
    Switch: `
    <strong>Switch:</strong> A networking device that connects multiple PCs or servers within 
    the same local area network (LAN). It uses MAC addresses to forward data efficiently.
    `,
    Router: `
    <strong>Router:</strong> A device that connects different networks and routes data 
    between them. It assigns IP addresses and directs packets to their destinations.
    `,
    Server: `
    <strong>Server:</strong> A system that provides data, resources, or services to 
    client devices (like PCs). It could host applications, files, or network configurations.
    `
};

// === Description update handler ===
const descriptionBox = document.querySelector(".Description p");

document.querySelectorAll(".device").forEach(device => {
    device.addEventListener("click", () => {
    const type = device.dataset.device;
    const desc = deviceDescriptions[type] || "No description available for this device.";
    descriptionBox.innerHTML = desc;
    });
});


function drawLine(x1, y1, x2, y2, color = "green", width = 2) {
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.strokeStyle = color;
    ctx.lineWidth = width;
    ctx.stroke();
}

const joinBtn = document.getElementById("join");
joinBtn.addEventListener("click", () => {
    joinBtn.classList.toggle("active");
    link = !link;
});

const canvas = document.querySelector(".SimulationArea");
const ctx = canvas.getContext("2d");
const logOutput = document.getElementById("logOutput");

let objects = [];
let dragging = null;
let offsetX = 0, offsetY = 0;

// Log helper
function addLog(message, type = "info") {
    const logEntry = document.createElement("div");
    logEntry.classList.add("log", type);
    const timestamp = new Date().toLocaleTimeString();
    let userName = "Zurus06";
    logEntry.textContent = `${userName}# ${message}`;
    logOutput.appendChild(logEntry);
    logOutput.scrollTop = logOutput.scrollHeight;
}

// Sync canvas with CSS
function fixCanvasResolution() {
    const rect = canvas.getBoundingClientRect();
    canvas.width = rect.width;
    canvas.height = rect.height;
    redraw();
}
fixCanvasResolution();
window.addEventListener("resize", fixCanvasResolution);

// Drag from sidebar
devices.forEach(device => {
    device.addEventListener("dragstart", e => {
    if (link) return;
    e.dataTransfer.setData("device", device.dataset.device);
    });
});

// Allow drop
canvas.addEventListener("dragover", e => {
    if (link) return;
    e.preventDefault()
});

document.getElementById("clearlog").addEventListener("click", ()=>{
    let logs = document.getElementById("logOutput");
    logs.replaceChildren();
});
// Drop → create object
canvas.addEventListener("drop", e => {
    if (link) return;
    e.preventDefault();
    const deviceType = e.dataTransfer.getData("device");
    if (!deviceType) return;

    // Get the rectangle bounds
    const rect = canvas.getBoundingClientRect();

    // e.clientX and e.clientY are the overall mouse positions on the browser page
    // so we subtract it from the left and right bounds of the rectangle to find the correct x, y coordinates of the item to be drawn
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    const nextId = deviceList[deviceType][deviceList[deviceType].length - 1] + 1;
    deviceList[deviceType].push(nextId);

    objects.push({
    Id: nextId,
    Type: deviceType,
    X: x - 20,
    Y: y - 20,
    Width: 40,
    Height: 40
    });

    addLog(`${deviceType} ${nextId} added to simulation area`, "success");
    redraw();
});

function drawDeviceIcon(ctx, obj) {
    ctx.save(); 
    ctx.translate(obj.X, obj.Y);

    switch (obj.Type) {
    case "PC":
        // Monitor
        ctx.fillStyle = "lightblue";
        ctx.fillRect(0, 0, obj.Width, obj.Height - 10);
        // Base
        ctx.fillStyle = "gray";
        ctx.fillRect(10, obj.Height - 10, obj.Width - 20, 10);
        break;

    case "Switch":
        ctx.fillStyle = "orange";
        ctx.fillRect(0, 0, obj.Width, obj.Height);
        ctx.fillStyle = "black";
        for (let i = 5; i < obj.Width; i += 10) {
        ctx.fillRect(i, obj.Height / 2 - 2, 4, 4); // ports
        }
        break;

    case "Router":
        ctx.fillStyle = "green";
        ctx.beginPath();
        ctx.arc(obj.Width / 2, obj.Height / 2, obj.Width / 2, 0, Math.PI * 2);
        ctx.fill();
        ctx.fillStyle = "white";
        ctx.fillText("R", obj.Width / 2 - 4, obj.Height / 2 + 4);
        break;

    case "Server":
        ctx.fillStyle = "purple";
        ctx.fillRect(0, 0, obj.Width, obj.Height);
        ctx.fillStyle = "white";
        ctx.fillText("SV", 5, obj.Height / 2 + 4);
        break;
    }

    ctx.restore(); // restore canvas state
}

// Redraw all objects
function redraw() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // Draw edges first
    edgeList.forEach(edge => {
    const from = edge.from;
    const to = edge.to;
    drawLine(from.X + from.Width / 2, from.Y + from.Height / 2,
            to.X + to.Width / 2,   to.Y + to.Height / 2);
    });

    objects.forEach(obj => {
    drawDeviceIcon(ctx, obj);
    ctx.fillStyle = "black";
    ctx.font = "12px monospace";
    ctx.fillText(`${obj.Type} ${obj.Id}`, obj.X, obj.Y + obj.Height + 12);
    });
}

// Hit detection
function getObjectAt(x, y) {
    return objects.find(obj =>
    x >= obj.X && x <= obj.X + obj.Width &&
    y >= obj.Y && y <= obj.Y + obj.Height
    );
}

// Mouse handling
canvas.addEventListener("mousedown", e => {
    e.preventDefault();
    const rect = canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;
    const obj = getObjectAt(x, y);
    if (!link) {
    if (e.button === 0 && obj) { 
        // Left click → start drag
        dragging = obj;
        offsetX = x - obj.X;
        offsetY = y - obj.Y;
    } else if (e.button === 2 && obj) {
        // Right click → delete
        objects = objects.filter(o => o !== obj);
        edgeList = edgeList.filter(edge => edge.from !== obj && edge.to !== obj);
        addLog(`${obj.Type} ${obj.Id} deleted`, "error");
        redraw();
    }
    }
    else {
    // Linking mode
    const selectedObject = getObjectAt(x, y);
    if (!selectedObject) return;

    if (start === null) {
        // First selection
        start = selectedObject;
    } else if (start.Id !== selectedObject.Id || start.Type !== selectedObject.Type) {
        // Second selection → create edge
        // But let's check if the link already exists
        const exists = edgeList.some(o =>
        (o.from === start && o.to === selectedObject) ||
        (o.to === start && o.from === selectedObject)
        );
        if (!exists) {
        edgeList.push({ from: start, to: selectedObject });
        addLog(`Linked ${start.Type} ${start.Id} → ${selectedObject.Type} ${selectedObject.Id}`, "success");
        start = null;
        redraw();
        } else {
        addLog(`Link already exists between ${start.Type} ${start.Id} and ${selectedObject.Type} ${selectedObject.Id}`, "error");
        start = null;
        }
    }
    }
});

canvas.addEventListener("mousemove", e => {
    if (!dragging) return;
    const rect = canvas.getBoundingClientRect();
    dragging.X = e.clientX - rect.left - offsetX;
    dragging.Y = e.clientY - rect.top - offsetY;

    redraw();
});

canvas.addEventListener("mouseup", () => {
    dragging = null;
});

canvas.addEventListener("contextmenu", e => e.preventDefault());