// canvas.js
// Uses your original drawing helpers with a small addition: findDevice(devices, type, id)

export function drawLine(ctx, x1, y1, x2, y2, color = "green", width = 2) {
  ctx.beginPath();
  ctx.moveTo(x1, y1);
  ctx.lineTo(x2, y2);
  ctx.strokeStyle = color;
  ctx.lineWidth = width;
  ctx.stroke();
}

export function drawDeviceIcon(ctx, obj) {
  ctx.save();
  ctx.translate(obj.X, obj.Y);

  switch (obj.Type) {
    case "PC":
      ctx.fillStyle = "lightblue";
      ctx.fillRect(0, 0, obj.Width, obj.Height - 10);
      ctx.fillStyle = "gray";
      ctx.fillRect(10, obj.Height - 10, obj.Width - 20, 10);
      break;

    case "Switch":
      ctx.fillStyle = "orange";
      ctx.fillRect(0, 0, obj.Width, obj.Height);
      ctx.fillStyle = "black";
      for (let i = 5; i < obj.Width; i += 10)
        ctx.fillRect(i, obj.Height / 2 - 2, 4, 4);
      break;

    case "Router":
      ctx.fillStyle = "green";
      ctx.beginPath();
      ctx.arc(obj.Width / 2, obj.Height / 2, obj.Width / 2, 0, Math.PI * 2);
      ctx.fill();
      ctx.fillStyle = "white";
      ctx.font = "bold 12px monospace";
      ctx.fillText("R", obj.Width / 2 - 4, obj.Height / 2 + 4);
      break;

    case "Server":
      ctx.fillStyle = "purple";
      ctx.fillRect(0, 0, obj.Width, obj.Height);
      ctx.fillStyle = "white";
      ctx.font = "bold 11px monospace";
      ctx.fillText("SV", 5, obj.Height / 2 + 4);
      break;
  }

  ctx.restore();
}

export function findDevice(devices, type, id) {
  // id may be stored as number or string; normalize
  const needleId = typeof id === "string" && /^\d+$/.test(id) ? parseInt(id, 10) : id;
  return devices.find(d => d.Type === type && d.Id === needleId);
}

export function redraw(ctx, canvas, objects, edgeList) {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  // draw edges (resolve device by type+id)
  edgeList.forEach(edge => {
    const from = findDevice(objects, edge.from.Type, edge.from.Id);
    const to = findDevice(objects, edge.to.Type, edge.to.Id);
    if (!from || !to) return;

    drawLine(
      ctx,
      from.X + from.Width / 2, from.Y + from.Height / 2,
      to.X + to.Width / 2, to.Y + to.Height / 2,
      "#88c", // subtle color
      3
    );
  });

  // draw devices
  objects.forEach(obj => {
    drawDeviceIcon(ctx, obj);
    ctx.fillStyle = "black";
    ctx.font = "12px monospace";
    const labelY = Math.min(obj.Y + obj.Height + 12, canvas.height - 5);
    ctx.fillText(`${obj.Type} ${obj.Id}`, obj.X, labelY);
  });
}

export function getObjectAt(objects, x, y) {
  return objects.find(obj =>
    x >= obj.X && x <= obj.X + obj.Width &&
    y >= obj.Y && y <= obj.Y + obj.Height
  );
}