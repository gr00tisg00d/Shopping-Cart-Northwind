document.addEventListener("DOMContentLoaded", function () {
    fetchProducts();
});
document.getElementById("CategoryId").addEventListener("change", (e) => {
    document.getElementById('product_rows').dataset['id'] = e.target.value;
    fetchProducts();
});
document.getElementById('Discontinued').addEventListener("change", (e) => {
    fetchProducts();
});

const cartModalElement = document.getElementById('cartModal');
const cartModal = new bootstrap.Modal(cartModalElement);

// delegated event listener
document.getElementById('product_rows').addEventListener("click", (e) => {
    const p = e.target.closest('tr.product');
    if (!p) {
        return;
    }

    if (p.classList.contains('product')) {
        e.preventDefault()
        // console.log(p.dataset['id']);
        if (document.getElementById('User').dataset['customer'].toLowerCase() == "true") {
            document.getElementById('ProductId').innerHTML = p.dataset['id'];
            document.getElementById('ProductName').innerHTML = p.dataset['name'];
            document.getElementById('UnitPrice').innerHTML = Number(p.dataset['price']).toFixed(2);
            display_total();
            cartModal.show();
        } else {
            // alert("Only signed in customers can add items to the cart");
            toast("Access Denied", "You must be signed in as a customer to access the cart.");
        }
    }
});
const toast = (header, message) => {
    document.getElementById('toast_header').innerHTML = header;
    document.getElementById('toast_body').innerHTML = message;
    bootstrap.Toast.getOrCreateInstance(document.getElementById('liveToast')).show();
}
const display_total = () => {
    const total = parseInt(document.getElementById('Quantity').value) * Number(document.getElementById('UnitPrice').innerHTML);
    document.getElementById('Total').innerHTML = numberWithCommas(total.toFixed(2));
}
// update total when cart quantity is changed
document.getElementById('Quantity').addEventListener("change", (e) => {
    display_total();
});
// function to display commas in number
const numberWithCommas = x => x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
async function fetchProducts() {
    const id = document.getElementById('product_rows').dataset['id'];
    const discontinued = document.getElementById('Discontinued').checked ? "" : "/discontinued/false";
    const { data: fetchedProducts } = await axios.get(`../../api/category/${id}/product${discontinued}`);
    // console.log(fetchedProducts);
    let product_rows = "";
    fetchedProducts.map(product => {
        const css = product.discontinued ? " discontinued" : "";
        product_rows +=
            `<tr class="product${css}" data-id="${product.productId}" data-name="${product.productName}" data-price="${product.unitPrice}">
        <td>${product.productName}</td>
        <td class="text-end">${product.unitPrice.toFixed(2)}</td>
        <td class="text-end">${product.unitsInStock}</td>
      </tr>`;
    });
    document.getElementById('product_rows').innerHTML = product_rows;
}
document.getElementById('addToCart').addEventListener("click", (e) => {
    // hide modal
    cartModal.hide();
    // use axios post to add item to cart
    const item = {
        "id": Number(document.getElementById('ProductId').innerHTML),
        "email": document.getElementById('User').dataset['email'],
        "qty": Number(document.getElementById('Quantity').value)
    };

    postCartItem(item);
});

async function postCartItem(item) {
    try {
        const res = await axios.post('../../api/addtocart', item);
        const productName = res.data?.product?.productName ?? document.getElementById('ProductName').innerHTML;
        toast("Product Added", `${productName} successfully added to cart.`);
    } catch (error) {
        const message = typeof error.response?.data === 'string'
            ? error.response.data
            : 'Unable to add that product to the cart.';
        toast("Cart Error", message);
    }
}