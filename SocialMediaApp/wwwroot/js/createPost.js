"use strict";

document.addEventListener("DOMContentLoaded", () => {
  handleCharacterCount();
  handleImageUpload();
});

function handleCharacterCount() {
  document.querySelectorAll("input[maxlength], textarea[maxlength]").forEach(input => {
    const maxLength = input.maxLength;

    // Find the associated counter
    const counter = input.parentElement.querySelector(".character-counter");

    if (!counter) {
      return;
    }

    function updateCounter() {
      const currentLength = input.value.length;

      counter.textContent = `${currentLength} / ${maxLength}`;
    }

    input.addEventListener("input", updateCounter);

    // Initialize counter
    updateCounter();
  });
}

function handleImageUpload() {
  const fileInput = document.querySelector(".image-files");
  const previewList = document.querySelector(".image-preview-list");
  const template = document.querySelector("#image-preview-template");
  
  let selectedFiles = [];
  let previewUrls = new Map();
  
  function renderPreviews() {
  
    // Clear existing previews and release their URLs
    previewUrls.forEach(url => URL.revokeObjectURL(url));
    previewUrls.clear();
  
    previewList.replaceChildren();
  
    // Clone the template for each selected file and set the image source
    selectedFiles.forEach((file, index) => {
      const clone = template.content.cloneNode(true);
  
      const li = clone.querySelector("li");
      const img = clone.querySelector("img");
  
      const imageUrl = URL.createObjectURL(file);
      previewUrls.set(index, imageUrl);
  
      img.src = imageUrl;
      img.alt = file.name;
  
      li.dataset.index = index;
  
      previewList.appendChild(clone);
    });
  
    // Update the actual file input
    const dataTransfer = new DataTransfer();
  
    selectedFiles.forEach(file => dataTransfer.items.add(file));
  
    fileInput.files = dataTransfer.files;
  }
  
  // Handle file selection
  fileInput.addEventListener("change", (event) => {
    selectedFiles = Array.from(event.target.files).slice(0, 4); // Limit to 4 files
  
    renderPreviews();
  });
  
  // Handle image removal
  previewList.addEventListener("click", (event) => {
    const button = event.target.closest(".remove-image");
  
    // If the clicked element is not a remove button, do nothing
    if (!button) {
      return;
    }
  
    const li = button.closest("li");
    const index = Number(li.dataset.index);
  
    selectedFiles.splice(index, 1);
  
    renderPreviews();
  });
};
