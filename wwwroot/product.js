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

// delegated event listener
document.getElementById('product_rows').addEventListener("click", (e) => {
    const p = e.target.closest('tr.product');
    if (!p) {
        return;
    }

    if (p.classList.contains('product')) {
        e.preventDefault();
        if (document.getElementById('User').dataset['customer'].toLowerCase() == "true") {
            const item = {
                "id": Number(p.dataset['id']),
                "email": document.getElementById('User').dataset['email'],
                "qty": 1,
                "name": p.dataset['name']
            };
            postCartItem(item);
        } else {
            toast("Access Denied", "You must be signed in as a customer to access the cart.");
        }
    }
});
const toast = (header, message) => {
    document.getElementById('toast_header').innerHTML = header;
    document.getElementById('toast_body').innerHTML = message;
    bootstrap.Toast.getOrCreateInstance(document.getElementById('liveToast')).show();
}

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
async function postCartItem(item) {
    try {
        const res = await axios.post('../../api/addtocart', item);
        const productName = res.data?.productName ?? item.name;
        const totalItems = res.data?.totalItems ?? '?';
        toast("Added to Cart", `${productName} has been added to your cart. You have ${totalItems} item${totalItems === 1 ? '' : 's'} in your cart.`);
    } catch (error) {
        toast("Cart Error", "Unable to add that product to the cart.");
    }
}