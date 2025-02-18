document.addEventListener('DOMContentLoaded', () => {
    let cartButtons = document.querySelectorAll('[data-cart-product]');
    for (let btn of cartButtons) {
        btn.addEventListener('click', addCartClick);
    }

    for (let btn of document.querySelectorAll('[data-cart-detail-dec]')) {
        btn.addEventListener('click', decrementCartClick);
    }

    for (let btn of document.querySelectorAll('[data-cart-detail-inc]')) {
        btn.addEventListener('click', incrementCartClick);
    }
});

function decrementCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest('[data-cart-detail-dec]');
    const cdId = cdElement.getAttribute('data-cart-detail-dec');
    console.log("- " +cdId);
}

function incrementCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest('[data-cart-detail-inc]');
    const cdId = cdElement.getAttribute('data-cart-detail-inc');
    console.log("+ " + cdId);
}

function addCartClick(e) {
    e.stopPropagation();
    const cartElement = e.target.closest('[data-cart-product]');
    const productId = cartElement.getAttribute('data-cart-product');
    console.log(productId);
    fetch('/Shop/AddToCart/' + productId, {
        method: 'PUT',
    }).then(r => r.json()).then(j => {
        console.log(j);
        if (j.status == 401) {
            openModal('Помилка', 'Увійдіть до системи для замовлення товарів');
            return;
        }
        else if (j.status == 400) {
            openModal('Помилка', 'Невірний формат ідентифікатора товару. Спробуйте ще раз.');
            return;
        }
        else if (j.status == 404) {
            openModal('Помилка', 'Обраний товар не знайдено. Можливо, він більше не доступний.');
            return;
        }
        else if (j.status == 201) {
            openModal('Успіх', 'Товар додано. Бажаєте перейти до свого кошику?', true);
            return;
        }
        else {
            openModal('Помилка', 'Щось пішло не так!');
            return;
        }
    });
}

document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id === "auth-form") {
        e.preventDefault();
        const login = form.querySelector('[name="UserLogin"]');
        const password = form.querySelector('[name="Password"]');
        const authError = document.getElementById('auth-error');
        authError.textContent = '';
        let isValid = true;

        if (login.value.length == 0) {
            login.classList.add("is-invalid");
            login.classList.remove("is-valid");
            form.querySelector("#login-feedback").innerText = "Введіть логін.";
            isValid = false;
        } else {
            login.classList.remove("is-invalid");
            login.classList.add("is-valid");
        }
        if (password.value.length == 0) {
            password.classList.add("is-invalid");
            password.classList.remove("is-valid");
            form.querySelector("#password-feedback").innerText = "Введіть пароль.";
            isValid = false;
        } else {
            password.classList.remove("is-invalid");
            password.classList.add("is-valid");
        }

        if (!isValid) {
            return;
        }

        const credentials = btoa(login.value + ':' + password.value);
        fetch("/User/Authenticate",
            {
                method: "GET",
                headers: {
                    'Authorization': 'Basic ' + credentials
                }
            }).then(r => {
                if (r.ok) {
                    window.location.reload();
                } else {
                    r.json().then(j => {
                        authError.textContent = j;
                    });
                }
            });
        console.log(credentials);
    }
})

const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl));

document.querySelectorAll('input[name="SlugOption"]').forEach(radio => {
    radio.addEventListener('change', function () {
        const customInput = document.getElementById('customSlugInput');
        if (this.value === 'custom') {
            customInput.style.display = 'block';
        } else {
            customInput.style.display = 'none';
        }
    });
});

function openModal(title, message, success = false) {
    const confirmButton = success ? `<button type="button" class="btn btn-primary" id="cart-btn" data-bs-dismiss="modal">Перейти до кошику</button>` : '';
    const modalHTML = `<div class="modal" id="cartModal" tabindex=" - 1">
                     <div class="modal-dialog">
                         <div class="modal-content">
                             <div class="modal-header">
                                 <h5 class="modal-title">${title}</h5>
                                 <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                             </div>
                             <div class="modal-body">
                                 <p>${message}</p>
                             </div>
                             <div class="modal-footer">
                                 <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Закрити</button>
                                 ${confirmButton}
                             </div>
                         </div>
                     </div>
                   </div>`;
    document.body.insertAdjacentHTML('beforeend', modalHTML);
    const modalWindow = new bootstrap.Modal(document.getElementById('cartModal'));
    modalWindow.show();
    if (success) {
        document.getElementById('cart-btn').addEventListener('click', function () {
            window.location = '/User/Cart';
        });
    }
    document.getElementById('cartModal').addEventListener('hidden.bs.modal', function () {
        document.getElementById('cartModal').remove();
    });
};