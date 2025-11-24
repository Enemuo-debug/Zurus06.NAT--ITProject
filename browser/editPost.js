const urlParams = new URLSearchParams(window.location.search);
const postId = urlParams.get("id");

const titleInput = document.getElementById("postTitle");
const introInput = document.getElementById("postContent");
const cardsContainer = document.getElementById("cardsContainer");
const saveBtn = document.getElementById("savePostBtn");

let mergedContents = [];

if (!postId) {
  alert("No post ID found. Redirecting...");
  window.location.href = "dashboard.html";
}

// === Fetch existing post ===
async function fetchPost() {
  try {
    const response = await fetch(`http://localhost:5245/posts/${postId}`, {
      method: "GET",
      headers: { "Content-Type": "application/json" },
      credentials: "include"
    });

    if (!response.ok) throw new Error("Failed to load post");

    const post = await response.json();

    titleInput.value = post.caption || "";
    introInput.value = post.intro || "";

    mergedContents = (post.contents || []);

    renderContentCards();
  } catch (err) {
    alert("Error loading post: " + err.message);
  }
}

document.getElementById("logout").addEventListener("click", async () => {
  try {
    const res = await fetch("http://localhost:5245/account/logout", {
      method: "GET",
      credentials: "include"
    });
    if (!res.ok) throw new Error("Logout failed");
    window.location.href = "signin.html";
  } catch (err) {
    alert("Error during logout: " + err.message);
  }
});

saveBtn.addEventListener("click", async () => {
  const caption = titleInput.value.trim();
  const intro = introInput.value.trim();
  saveBtn.disabled = true;
  saveBtn.textContent = "Saving...";

  try {
    const finalContentIds = [];

    for (let i = 0; i < mergedContents.length; i++) {
      const item = mergedContents[i];

      if (item.isNew) {
        const formData = new FormData();

        if ((item.$type || item.type) === "image" && item.file) {
          formData.append("type", "Image");
          formData.append("Content", item.content || " ");
          formData.append("File", item.file);
        } else if ((item.$type || item.type) === "text") {
          formData.append("type", "Text");
          formData.append("Content", item.content || "");
        }else if ((item.$type || item.type) === "net") {
          formData.append("type", "NATSimulation");
          formData.append("simUUID", item.code);
        }
        else {
          formData.append("type", "Text");
          formData.append("Content", item.content || "");
        }

        const res = await fetch(`http://localhost:5245/posts/new-content`, {
          method: "POST",
          credentials: "include",
          body: formData
        });

        if (!res.ok) {
          const errtxt = await res.text();
          throw new Error(`Failed to save new content: ${errtxt}`);
        }

        const saved = await res.json();

        item.id = saved.id;
        item.isNew = false;
        finalContentIds.push(item.id);
      } else {
        if (!item.id) {
          throw new Error("Existing content missing id at index " + i);
        }
        finalContentIds.push(item.id);
      }
    }

    const res = await fetch(`http://localhost:5245/posts/edit/${postId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      credentials: "include",
      body: JSON.stringify({ caption, intro, contents: finalContentIds }),
    });

    if (!res.ok) {
      const errtext = await res.text();
      throw new Error("Failed to save post changes: " + errtext);
    }

    alert("✅ Post updated successfully!");
    window.location.href = "dashboard.html";
  } catch (err) {
    alert("Error saving post: " + err.message);
  } finally {
    saveBtn.disabled = false;
    saveBtn.textContent = "Save Changes";
  }
});

let selectedCardIndex = null;
const modal = document.getElementById("contentModal");
const openModalBtn = document.getElementById("openModalBtn");
const closeModalBtn = document.getElementById("closeModal");
const contentTypeSelect = document.getElementById("contentType");

openModalBtn.addEventListener("click", () => (modal.style.display = "flex"));
closeModalBtn.addEventListener("click", () => (modal.style.display = "none"));

contentTypeSelect.addEventListener("change", () => {
  const type = contentTypeSelect.value;
  document.getElementById("textInputGroup").style.display = type === "text" ? "" : "none";
  document.getElementById("imageInputGroup").style.display = type === "image" ? "" : "none";
  document.getElementById("simInputGroup").style.display = type === "simulation" ? "" : "none";
});

document.getElementById("addContentBtn").addEventListener("click", async () => {
  const type = contentTypeSelect.value;
  let newContent = null;

  if (type === "text") {
    const textData = document.getElementById("contentData").value.trim();
    if (!textData) return alert("Please enter text content.");
    newContent = { isNew: true, $type: "text", content: textData };
  } 
  else if (type === "image") {
    const fileInput = document.getElementById("imgUpload");
    const caption = document.getElementById("imgCaption").value.trim() || "...";
    if (!fileInput.files[0]) return alert("Please select an image.");

    const imgURL = URL.createObjectURL(fileInput.files[0]);
    newContent = { isNew: true, $type: "image", imgLink: imgURL, content: caption, file: fileInput.files[0] };
  } 
  else if (type === "simulation") {
      const uuid = document.getElementById("simUUID").value.trim();
      if (!uuid) return alert("Please enter the simulation UUID.");

      // Validate UUID with backend
      const isValidJson = await fetch(
          `http://localhost:5245/diagrams/verify-embed/${postId}`,
          {
              method: "POST",
              credentials: "include",
              headers: { "Content-Type": "application/json" },
              body: JSON.stringify(uuid)
          }
      );

      const isValid = await isValidJson.json();
      if (!isValid.success) return alert("Invalid simulation embed");

      newContent = { 
          isNew: true, 
          $type: "net", 
          simUUID: isValid.data,
          code: uuid
      };
  }  
   else {
    return alert("Unknown type selected.");
  }

  mergedContents.push(newContent);
  renderContentCards();

  document.getElementById("createPostForm").reset();
  modal.style.display = "none";
});

function renderContentCards() {
  cardsContainer.innerHTML = "";

  mergedContents.forEach((content, index) => {
    const type = (content.$type || content.type || "").toLowerCase();
    const card = document.createElement("div");
    card.className = "content-card";
    card.draggable = false;

    // Build inner HTML safely (we're controlling values here)
    if (type === "text") {
      const safeText = escapeHtml(content.content || "");
      card.innerHTML = `
        <p><b>Text:</b> ${safeText}</p>
        <div class="card-buttons">
          <button data-index="${index}" class="delete-btn">Delete</button>
          <button class="swapUp">MOVE UP ⤊</button>
          <button class="swapDown">MOVE DOWN ⤋</button>
        </div>
      `;
    } 
    else if (type === "image") {
      const imgSrc = content.imgLink || "";
      const safeCap = escapeHtml(content.content || "");
      card.innerHTML = `
        <img src="${imgSrc}" alt="Uploaded Image" width="120">
        <p><b>Caption:</b> ${safeCap}</p>
        <div>
          <button data-index="${index}" class="delete-btn">Delete</button>
          <button class="swapUp">MOVE UP ⤊</button>
          <button class="swapDown">MOVE DOWN ⤋</button>
        </div>
      `;
    } 
    else if (type === "net") {
      const uuid = escapeHtml(content.natSimulation || "");
      const parts = uuid.split("^");
      const simName = parts[parts.length - 1];

      card.innerHTML = `
        <p><b>Simulation Name: ${simName}</b></p>
        <canvas class="SimulationArea"></canvas>
        <div>
          <button data-index="${index}" class="delete-btn">Delete</button>
          <button class="swapUp">MOVE UP ⤊</button>
          <button class="swapDown">MOVE DOWN ⤋</button>
        </div>
      `;
    }

    else {
      const json = escapeHtml(JSON.stringify(content));
      card.innerHTML = `
        <p><b>Unknown Type:</b> ${json}</p>
        <button data-index="${index}" class="delete-btn">Delete</button>
      `;
    }
    cardsContainer.appendChild(card);
  });

  document.querySelectorAll(".swapUp").forEach((btn, idx) => {
    btn.addEventListener("click", () => {
      if (idx <= 0) return;
      const tmp = mergedContents[idx - 1];
      mergedContents[idx - 1] = mergedContents[idx];
      mergedContents[idx] = tmp;
      renderContentCards();
    });
  });

  document.querySelectorAll(".swapDown").forEach((btn, idx) => {
    btn.addEventListener("click", () => {
      if (idx >= mergedContents.length - 1) return;
      const tmp = mergedContents[idx + 1];
      mergedContents[idx + 1] = mergedContents[idx];
      mergedContents[idx] = tmp;
      renderContentCards();
    });
  });

  // attach delete handlers (delegation-like)
  cardsContainer.querySelectorAll(".delete-btn").forEach(btn => {
    btn.addEventListener("click", (e) => {
      const idx = Number(btn.getAttribute("data-index"));
      deleteContent(idx);
    });
  });
}

// === Delete content ===
function deleteContent(index) {
  if (index < 0 || index >= mergedContents.length) return;
  const removed = mergedContents.splice(index, 1)[0];

  // If it was a newly-created image and we created an object URL, revoke it to avoid leaks
  if (removed && removed.$type === "image" && removed.imgLink && removed.isNew) {
    try { URL.revokeObjectURL(removed.imgLink); } catch (e) { /* ignore */ }
  }

  renderContentCards();
}

// small helper to escape text inserted into innerHTML
function escapeHtml(text) {
  if (!text) return "";
  return text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
}

// Initial fetch
fetchPost();