const locationButton = document.querySelector("#use-current-location");
const mapLinkInput = document.querySelector("#MapLink");
const locationMessage = document.querySelector("#location-message");

if (locationButton && mapLinkInput && locationMessage) {
    locationButton.addEventListener("click", () => {
        if (!navigator.geolocation) {
            locationMessage.textContent = "Location is unavailable in this browser. Paste a map link instead.";
            return;
        }

        locationButton.disabled = true;
        locationMessage.textContent = "Waiting for location permission...";
        navigator.geolocation.getCurrentPosition(
            (position) => {
                const { latitude, longitude } = position.coords;
                mapLinkInput.value = `https://www.google.com/maps?q=${latitude},${longitude}`;
                locationMessage.textContent = "Map link added. You can still edit it.";
                locationButton.disabled = false;
            },
            () => {
                locationMessage.textContent = "Location not shared. You can paste a map link instead.";
                locationButton.disabled = false;
            },
            { enableHighAccuracy: false, timeout: 10000, maximumAge: 60000 }
        );
    });
}

const photoInput = document.querySelector("#photos");
const photoPreview = document.querySelector("#photo-preview");

if (photoInput && photoPreview) {
    photoInput.addEventListener("change", () => {
        const files = Array.from(photoInput.files || []);
        const existingCount = Number(photoInput.dataset.existingCount || 0);
        const invalidFile = files.find((file) =>
            !["image/jpeg", "image/png", "image/webp"].includes(file.type) || file.size > 5 * 1024 * 1024
        );

        if (files.length + existingCount > 5 || invalidFile) {
            photoInput.value = "";
            photoPreview.textContent = files.length + existingCount > 5
                ? "Choose no more than five photos in total."
                : "Choose JPG, PNG or WebP photos under 5 MB each.";
            return;
        }

        photoPreview.replaceChildren();
        for (const file of files) {
            const image = document.createElement("img");
            image.src = URL.createObjectURL(file);
            image.alt = "Selected property photo";
            photoPreview.append(image);
        }
    });
}
