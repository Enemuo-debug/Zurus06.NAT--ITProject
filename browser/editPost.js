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

    mergedContents = (post.contents || []).map(c => {
      return {
        id: c.id,
        isNew: false,
        $type: (c.type || c.$type || "text").toLowerCase(),
        content: c.content || c.text || "",
        imgLink: c.imgLink || c.url || "",
      };
    });

    renderContentCards();
    console.log("Loaded post with contents:", mergedContents);
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
        } else if ((item.$type || item.type) === "natsimulation" || (item.$type || item.type) === "simulation") {
          formData.append("type", "NATSimulation");
          formData.append("simUUID", item.simUUID || item.simUUID);
        } else {
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

    // Now update the post with ordered IDs
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
    console.error(err);
  } finally {
    saveBtn.disabled = false;
    saveBtn.textContent = "Save Changes";
  }
});

// modal / content creation UI elements
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

// === Add new content ===
document.getElementById("addContentBtn").addEventListener("click", () => {
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
    newContent = { isNew: true, $type: "natsimulation", simUUID: uuid };
  } else {
    return alert("Unknown type selected.");
  }

  // push into mergedContents (keeps UI ordering simple)
  mergedContents.push(newContent);
  console.log("New content added:", newContent);
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
    card.draggable = true;

    // Build inner HTML safely (we're controlling values here)
    if (type === "text") {
      const safeText = escapeHtml(content.content || "");
      card.innerHTML = `
        <p><b>Text:</b> ${safeText}</p>
        <button data-index="${index}" class="delete-btn">Delete</button>
      `;
    } 
    else if (type === "image") {
      const imgSrc = content.imgLink || "";
      const safeCap = escapeHtml(content.content || "");
      card.innerHTML = `
        <img src="${imgSrc}" alt="Uploaded Image" width="120">
        <p><b>Caption:</b> ${safeCap}</p>
        <button data-index="${index}" class="delete-btn">Delete</button>
      `;
    } 
    else if (type === "natsimulation" || type === "simulation") {
      const uuid = escapeHtml(content.simUUID || content.NATSimulation || "");
      card.innerHTML = `
        <p><b>Simulation UUID:</b> ${uuid}</p>
        <button data-index="${index}" class="delete-btn">Delete</button>
      `;
    } 
    else {
      const json = escapeHtml(JSON.stringify(content));
      card.innerHTML = `
        <p><b>Unknown Type:</b> ${json}</p>
        <button data-index="${index}" class="delete-btn">Delete</button>
      `;
    }

    // drag handlers
    card.addEventListener("dragstart", (e) => {
      selectedCardIndex = index;
      e.dataTransfer.effectAllowed = "move";
    });

    card.addEventListener("dragover", (e) => {
      e.preventDefault();
    });

    card.addEventListener("drop", (e) => {
      e.preventDefault();
      if (selectedCardIndex === null || selectedCardIndex === index) return;

      // swap in mergedContents
      const tmp = mergedContents[selectedCardIndex];
      mergedContents[selectedCardIndex] = mergedContents[index];
      mergedContents[index] = tmp;

      selectedCardIndex = null;
      renderContentCards();
    });

    cardsContainer.appendChild(card);
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