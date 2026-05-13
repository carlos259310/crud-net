document.addEventListener("DOMContentLoaded", () => {

    const tiposDocumentosUrl = "http://localhost:5117/api/tiposdocumentos";

    const form = document.getElementById("tiposdocumentosForm");
    const tipoDocumentoId = document.getElementById("tipodocumentoId");
    const codigo = document.getElementById("codigo");
    const nombre = document.getElementById("nombre");
    const table = document.getElementById("tipodocumentoTable");

    async function loadTiposDocumentos() {
        const response = await fetch(tiposDocumentosUrl);
        const tipos = await response.json();

        table.innerHTML = "";

        tipos.forEach(tipo => {
            table.innerHTML += `
                <tr>
                    <td>${tipo.id}</td>
                    <td>${tipo.codigo}</td>
                    <td>${tipo.nombre}</td>
                    <td>
                        <button class="btn btn-warning btn-sm"
                            onclick='editTipoDocumento(${JSON.stringify(tipo)})'>
                            Editar
                        </button>

                        <button class="btn btn-danger btn-sm"
                            onclick="deleteTipoDocumento(${tipo.id})">
                            Eliminar
                        </button>
                    </td>
                </tr>
            `;
        });
    }

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const tipoDocumento = {
            codigo: codigo.value,
            nombre: nombre.value
        };

        const id = tipoDocumentoId.value;

        if (id) {
            await fetch(`${tiposDocumentosUrl}/${id}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(tipoDocumento)
            });
        } else {
            await fetch(tiposDocumentosUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(tipoDocumento)
            });
        }

        form.reset();
        tipoDocumentoId.value = "";
        loadTiposDocumentos();
    });

    window.editTipoDocumento = function (tipo) {
        tipoDocumentoId.value = tipo.id;
        codigo.value = tipo.codigo;
        nombre.value = tipo.nombre;
    };

    window.deleteTipoDocumento = async function (id) {
        const confirmar = confirm("¿Desea eliminar el tipo de documento?");
        if (!confirmar) return;

        await fetch(`${tiposDocumentosUrl}/${id}`, { method: "DELETE" });
        loadTiposDocumentos();
    };

    loadTiposDocumentos();
});