document.addEventListener("DOMContentLoaded", () => {

    const categoriasUrl = "http://localhost:5117/api/categorias";

    const form = document.getElementById("categoriaForm");
    const categoriaId = document.getElementById("categoriaId");
    const nombre = document.getElementById("nombre");
    const table = document.getElementById("categoriasTable");

    async function loadCategorias() {
        const response = await fetch(categoriasUrl);
        const categorias = await response.json();

        table.innerHTML = "";

        categorias.forEach(categoria => {
            table.innerHTML += `
                <tr>
                    <td>${categoria.id}</td>
                    <td>${categoria.nombre}</td>
                    <td>
                        <button class="btn btn-warning btn-sm"
                            onclick='editCategoria(${JSON.stringify(categoria)})'>
                            Editar
                        </button>

                        <button class="btn btn-danger btn-sm"
                            onclick="deleteCategoria(${categoria.id})">
                            Eliminar
                        </button>
                    </td>
                </tr>
            `;
        });
    }

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const categoria = {
            nombre: nombre.value
        };

        const id = categoriaId.value;

        if (id) {
            await fetch(`${categoriasUrl}/${id}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(categoria)
            });
        } else {
            await fetch(categoriasUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(categoria)
            });
        }

        form.reset();
        categoriaId.value = "";
        loadCategorias();
    });

    window.editCategoria = function (categoria) {
        categoriaId.value = categoria.id;
        nombre.value = categoria.nombre;
    };

    window.deleteCategoria = async function (id) {
        const confirmar = confirm("¿Desea eliminar la categoría?");
        if (!confirmar) return;

        await fetch(`${categoriasUrl}/${id}`, { method: "DELETE" });
        loadCategorias();
    };

    loadCategorias();
});