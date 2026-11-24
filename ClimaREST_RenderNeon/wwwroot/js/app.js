const apiUrl = "/api/weatherforecast";
const tableBody = document.getElementById("forecast-table");
const form = document.getElementById("forecast-form");
const feedback = document.getElementById("form-feedback");
const saveButton = document.getElementById("save-button");
let forecasts = [];

const weatherSymbols = {
  "Soleado": "☀", "Despejado": "☼", "Parcialmente nublado": "⛅",
  "Nublado": "☁", "Lluvioso": "☂", "Tormenta": "ϟ", "Brisa": "≋", "Niebla": "〰"
};

function formatDate(dateValue) {
  const [year, month, day] = dateValue.split("-").map(Number);
  return new Date(year, month - 1, day).toLocaleDateString("es-CO", {
    day: "2-digit", month: "short", year: "numeric"
  });
}

function showFeedback(message, isError = false) {
  feedback.textContent = message;
  feedback.classList.toggle("error", isError);
}

function updateSummary() {
  document.getElementById("total-count").textContent = forecasts.length;
  if (forecasts.length === 0) {
    document.getElementById("average-temp").textContent = "—";
    document.getElementById("warmest-temp").textContent = "—";
    document.getElementById("warmest-date").textContent = "Sin datos disponibles";
    return;
  }

  const average = forecasts.reduce((sum, item) => sum + item.temperatureC, 0) / forecasts.length;
  const warmest = forecasts.reduce((best, item) => item.temperatureC > best.temperatureC ? item : best);
  document.getElementById("average-temp").textContent = `${average.toFixed(1)} °C`;
  document.getElementById("warmest-temp").textContent = `${warmest.temperatureC} °C`;
  document.getElementById("warmest-date").textContent = formatDate(warmest.date);
}

function renderForecasts() {
  tableBody.replaceChildren();
  if (forecasts.length === 0) {
    const row = document.createElement("tr");
    const cell = document.createElement("td");
    cell.colSpan = 4;
    cell.className = "empty-cell";
    cell.textContent = "Todavía no hay pronósticos. Añade el primero desde el formulario.";
    row.appendChild(cell);
    tableBody.appendChild(row);
    return;
  }

  forecasts.forEach((forecast) => {
    const row = document.createElement("tr");
    const dateCell = document.createElement("td");
    dateCell.className = "date-cell";
    dateCell.textContent = formatDate(forecast.date);

    const summaryCell = document.createElement("td");
    const tag = document.createElement("span");
    tag.className = "weather-tag";
    const symbol = document.createElement("span");
    symbol.className = "weather-symbol";
    symbol.textContent = weatherSymbols[forecast.summary] || "◌";
    const label = document.createElement("span");
    label.textContent = forecast.summary;
    tag.append(symbol, label);
    summaryCell.appendChild(tag);

    const temperatureCell = document.createElement("td");
    temperatureCell.className = "temp-cell";
    temperatureCell.textContent = `${forecast.temperatureC} °C`;
    const fahrenheit = document.createElement("small");
    fahrenheit.textContent = `(${forecast.temperatureF} °F)`;
    temperatureCell.appendChild(fahrenheit);

    const actionsCell = document.createElement("td");
    const actions = document.createElement("div");
    actions.className = "row-actions";
    const editButton = document.createElement("button");
    editButton.type = "button";
    editButton.className = "icon-button";
    editButton.title = "Editar pronóstico";
    editButton.setAttribute("aria-label", "Editar pronóstico");
    editButton.textContent = "✎";
    editButton.addEventListener("click", () => startEditing(forecast));

    const deleteButton = document.createElement("button");
    deleteButton.type = "button";
    deleteButton.className = "icon-button delete";
    deleteButton.title = "Eliminar pronóstico";
    deleteButton.setAttribute("aria-label", "Eliminar pronóstico");
    deleteButton.textContent = "⌫";
    deleteButton.addEventListener("click", () => deleteForecast(forecast.id));

    actions.append(editButton, deleteButton);
    actionsCell.appendChild(actions);
    row.append(dateCell, summaryCell, temperatureCell, actionsCell);
    tableBody.appendChild(row);
  });
}

async function loadForecasts() {
  tableBody.innerHTML = '<tr><td colspan="4" class="empty-cell">Consultando la base de datos...</td></tr>';
  try {
    const response = await fetch(apiUrl);
    if (!response.ok) throw new Error("No fue posible consultar la API.");
    forecasts = await response.json();
    renderForecasts();
    updateSummary();
    document.getElementById("last-updated").textContent = `Actualizado ${new Date().toLocaleTimeString("es-CO", { hour: "2-digit", minute: "2-digit" })}`;
  } catch (error) {
    tableBody.innerHTML = '<tr><td colspan="4" class="empty-cell">No se pudo conectar con el servicio. Recarga la página para intentar de nuevo.</td></tr>';
    showFeedback(error.message, true);
  }
}

function resetForm() {
  form.reset();
  document.getElementById("forecast-id").value = "";
  document.getElementById("forecast-temperature").value = "22";
  document.getElementById("form-title").textContent = "Añadir pronóstico";
  document.getElementById("form-eyebrow").textContent = "NUEVO REGISTRO";
  saveButton.innerHTML = 'Guardar pronóstico <span aria-hidden="true">↗</span>';
  document.getElementById("cancel-edit").classList.add("hidden");
  showFeedback("");
}

function startEditing(forecast) {
  document.getElementById("forecast-id").value = forecast.id;
  document.getElementById("forecast-date").value = forecast.date;
  document.getElementById("forecast-summary").value = forecast.summary;
  document.getElementById("forecast-temperature").value = forecast.temperatureC;
  document.getElementById("form-title").textContent = "Editar pronóstico";
  document.getElementById("form-eyebrow").textContent = `REGISTRO #${forecast.id}`;
  saveButton.innerHTML = 'Guardar cambios <span aria-hidden="true">↗</span>';
  document.getElementById("cancel-edit").classList.remove("hidden");
  showFeedback("Modifica los campos y guarda los cambios.");
  document.getElementById("forecast-date").focus();
  document.querySelector(".form-panel").scrollIntoView({ behavior: "smooth", block: "center" });
}

async function deleteForecast(id) {
  if (!window.confirm("¿Seguro que deseas eliminar este pronóstico? Esta acción no se puede deshacer.")) return;
  try {
    const response = await fetch(`${apiUrl}/${id}`, { method: "DELETE" });
    if (!response.ok) throw new Error("No se pudo eliminar el registro.");
    await loadForecasts();
    showFeedback("Pronóstico eliminado correctamente.");
  } catch (error) {
    showFeedback(error.message, true);
  }
}

form.addEventListener("submit", async (event) => {
  event.preventDefault();
  const id = document.getElementById("forecast-id").value;
  const payload = {
    date: document.getElementById("forecast-date").value,
    temperatureC: Number(document.getElementById("forecast-temperature").value),
    summary: document.getElementById("forecast-summary").value
  };

  if (!payload.date || !payload.summary || !Number.isInteger(payload.temperatureC)) {
    showFeedback("Completa todos los campos correctamente.", true);
    return;
  }

  saveButton.disabled = true;
  saveButton.textContent = "Guardando...";
  try {
    const response = await fetch(id ? `${apiUrl}/${id}` : apiUrl, {
      method: id ? "PUT" : "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      const problem = await response.json().catch(() => null);
      const message = problem?.errors
        ? Object.values(problem.errors).flat().join(" ")
        : "Verifica los datos e inténtalo de nuevo.";
      throw new Error(message);
    }

    resetForm();
    await loadForecasts();
    showFeedback(id ? "Cambios guardados correctamente." : "Pronóstico añadido correctamente.");
  } catch (error) {
    showFeedback(error.message, true);
  } finally {
    saveButton.disabled = false;
    if (document.getElementById("forecast-id").value) {
      saveButton.innerHTML = 'Guardar cambios <span aria-hidden="true">↗</span>';
    } else {
      saveButton.innerHTML = 'Guardar pronóstico <span aria-hidden="true">↗</span>';
    }
  }
});

document.getElementById("cancel-edit").addEventListener("click", resetForm);
document.getElementById("refresh-button").addEventListener("click", loadForecasts);
loadForecasts();
