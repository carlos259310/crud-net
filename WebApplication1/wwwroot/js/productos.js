document.addEventListener("DOMContentLoaded", () => {

    const productosUrl = "http://localhost:5117/api/productos";
    const categoriasUrl = "http://localhost:5117/api/categorias";

    const form = document.getElementById("productoForm");

    const productoId = document.getElementById("productoId");

    const nombre = document.getElementById("nombre");

    const descripcion = document.getElementById("descripcion");

    const precio = document.getElementById("precio");

    const stock = document.getElementById("stock");

    const categoriaId = document.getElementById("categoriaId");

    const activo = document.getElementById("activo");

    const table = document.getElementById("productosTable");


    // ===============================
    // CARGAR CATEGORIAS
    // ===============================
    async function loadCategorias() {

        const response = await fetch(categoriasUrl);

        const categorias = await response.json();

        categoriaId.innerHTML = `
            <option value="">Seleccione</option>
        `;

        categorias.forEach(categoria => {

            categoriaId.innerHTML += `
                <option value="${categoria.id}">
                    ${categoria.nombre}
                </option>
            `;
        });
    }


    // ===============================
    // CARGAR PRODUCTOS
    // ===============================
    async function loadProductos() {

        const response = await fetch(productosUrl);

        const productos = await response.json();

        const categoriasResponse = await fetch(categoriasUrl);

        const categorias = await categoriasResponse.json();

        table.innerHTML = "";

        productos.forEach(producto => {

            const categoria = categorias.find(
                x => x.id === producto.categoriaId
            );

            table.innerHTML += `
                <tr>
                    <td>${producto.id}</td>
                    <td>${producto.nombre}</td>
                    <td>$ ${producto.precio}</td>
                    <td>${producto.stock}</td>
                    <td>${categoria?.nombre ?? ""}</td>
                    <td>
                        ${producto.activo ? "Sí" : "No"}
                    </td>

                    <td>

                        <button class="btn btn-warning btn-sm"
                            onclick='editProducto(${JSON.stringify(producto)})'>
                            Editar
                        </button>

                        <button class="btn btn-danger btn-sm"
                            onclick="deleteProducto(${producto.id})">
                            Eliminar
                        </button>

                    </td>
                </tr>
            `;
        });
    }


    // ===============================
    // GUARDAR
    // ===============================
    form.addEventListener("submit", async (e) => {

        e.preventDefault();

        const producto = {
            nombre: nombre.value,
            descripcion: descripcion.value,
            precio: parseFloat(precio.value),
            stock: parseInt(stock.value),
            categoriaId: parseInt(categoriaId.value),
            activo: activo.checked
        };

        const id = productoId.value;

        // EDITAR
        if (id) {

            await fetch(`${productosUrl}/${id}`, {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(producto)
            });

        }
        // CREAR
        else {

            await fetch(productosUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(producto)
            });
        }

        form.reset();

        productoId.value = "";

        loadProductos();
    });


    // ===============================
    // EDITAR
    // ===============================
    window.editProducto = function (producto) {

        productoId.value = producto.id;

        nombre.value = producto.nombre;

        descripcion.value = producto.descripcion;

        precio.value = producto.precio;

        stock.value = producto.stock;

        categoriaId.value = producto.categoriaId;

        activo.checked = producto.activo;
    }


    // ===============================
    // ELIMINAR
    // ===============================
    window.deleteProducto = async function (id) {

        const confirmar = confirm(
            "¿Desea eliminar el producto?"
        );

        if (!confirmar)
            return;

        await fetch(`${productosUrl}/${id}`, {
            method: "DELETE"
        });

        loadProductos();
    }


    // ===============================
    // INICIALIZAR
    // ===============================
    loadCategorias();

    loadProductos();

});