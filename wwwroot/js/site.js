document.addEventListener('DOMContentLoaded', () => {
    let cartButtons = document.querySelectorAll('[data-cart-product]');
    for (let btn of cartButtons) {
        btn.addEventListener('click', addCartClick);
    }

    for (let btn of document.querySelectorAll('[data-cart-detail-del]')) {
        btn.addEventListener('click', deleteCartClick);
    }

    for (let btn of document.querySelectorAll('[data-cart-detail-qnt]')) {
        btn.addEventListener('keydown', editCartEdit);
        btn.addEventListener('blur', editCartBlur);
        btn.addEventListener('focus', editCartFocus);
    }

    for (let btn of document.querySelectorAll('[data-cart-detail-dec]')) {
        btn.addEventListener('click', decrementCartClick);
    }

    for (let btn of document.querySelectorAll('[data-cart-detail-inc]')) {
        btn.addEventListener('click', incrementCartClick);
    }

    for (let btn of document.querySelectorAll('[data-cart-cancel]')) {
        btn.addEventListener('click', cancelCart);
    }

    for (let btn of document.querySelectorAll('[data-cart-buy]')) {
        btn.addEventListener('click', buyCart);
    }

    for (let btn of document.querySelectorAll('[data-cart-repeat]')) {
        btn.addEventListener('click', repeatCart);
    }

    const rateButton = document.getElementById('rate-button');
    if (rateButton) {
        rateButton.addEventListener('click', rateClick);
    }

    const backBtn = document.getElementById('back-btn');
    if (backBtn) {
        backBtn.addEventListener('click', getBack);
    }
});

function rateClick(e) {
    e.preventDefault();

    const btn = e.target.closest('[data-rate-user]');
    const userId = btn.getAttribute('data-rate-user');
    const productId = btn.getAttribute('data-rate-product');
    const commentInput = document.getElementById("rate-comment");
    const ratingInput = document.querySelector('input[name="Rate"]:checked');

    const rating = ratingInput ? ratingInput.value : null;
    const comment = commentInput.value.trim();

    document.querySelectorAll(".is-invalid").forEach(el => el.classList.remove("is-invalid"));
    document.querySelectorAll(".invalid-feedback").innerText = "";

    fetch("/Shop/Rate", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userId, productId, comment, rating })
    })
        .then(r => r.json())
        .then(j => {
            if (j.status >= 400) {
                if (j.errors.comment) {
                    showError(commentInput, j.errors.comment);
                }
                if (j.errors.rating) {
                    showError(document.querySelector(".star-rate"), j.errors.rating);
                }
                return;
            }
            window.location.reload();
        });
}

function showError(input, message) {
    input.classList.add("is-invalid");
    const rateFeedback = input.parentNode.querySelector("#rate-feedback");
    rateFeedback.innerText = message;
}

function starClick(e) {
    const star = e.target;
}

async function cancelCart(e) {
    const idElement = e.target.closest('[data-cart-cancel]');
    if (!idElement) throw "cancelCart() error: [data-cart-cancel] not found";
    const cartId = idElement.getAttribute('data-cart-cancel');
    if (!cartId) throw "cancelCart() error: [data-cart-cancel] attribute empty or not found";

    const modalResult = await openModal("Скасування", "Ви збираєтеся скасувати кошик. Підтверджуєте?", true);
    if (!modalResult) {
        return;
    }

    fetch(`/Shop/CloseCart/${cartId}`, {
        method: 'DELETE',
    }).then(r => r.json()).then(j => {
        console.log(j);
        if (j.status < 300) {
            window.location.reload();
            return;
        }
        else {
            alert(j.message);
            return;
        }
    });
}

async function buyCart(e) {
    const idElement = e.target.closest('[data-cart-buy]');
    if (!idElement) throw "buyCart() error: [data-cart-buy] not found";
    const cartId = idElement.getAttribute('data-cart-buy');
    if (!cartId) throw "buyCart() error: [data-cart-buy] attribute empty or not found";

    const modalResult = await openModal("Придбання", "Ви збираєтеся придбати кошик. Підтверджуєте?", true);
    if (!modalResult) {
        return;
    }

    fetch(`/Shop/CloseCart/${cartId}`, {
        method: 'DELETE',
        headers: {
            'Cart-Action': 'Buy',
        }
    }).then(r => r.json()).then(j => {
        console.log(j);
        if (j.status < 300) {
            window.location.reload();
            return;
        }
        else {
            alert(j.message);
            return;
        }
    });
}

async function repeatCart(e) {
    e.stopPropagation();
    const idElement = e.target.closest('[data-cart-repeat]');
    const cartId = idElement.getAttribute('data-cart-repeat');

    fetch(`/Shop/RepeatCart/${cartId}`, {
        method: "POST",
    })
        .then(r => r.json())
        .then(async j => {
            if (j.status == 401) {
                await openModal("Помилка", "Увійдіть до системи для повторення замовлення.");
                return;
            }
            else if (j.status == 400) {
                await openModal("Помилка", "Невірний формат ідентифікатора кошика.");
                return;
            }
            else if (j.status == 404) {
                await openModal("Помилка", "Вказаний кошик не знайдено.");
                return;
            }
            else if (j.status == 200) {
                let message = "Ваше замовлення успішно повторено!";

                if (j.warnings && j.warnings.length > 0) {
                    message += "\n\n⚠️ Деякі товари були обмежені або відсутні:\n\n";
                    message += j.warnings.join("\n");
                }

                await openModal("Успіх", message);
                window.location.reload();
                return;
            }
            else {
                await openModal("Помилка", "Щось пішло не так!");
                return;
            }
        });
}

function getBack(e) {
    e.stopPropagation();
    history.back();
}

async function deleteCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest('[data-cart-detail-del]');
    const cdId = cdElement.getAttribute('data-cart-detail-del');

    const productNameElement = e.target.closest('.card-detail')?.querySelector('#product-name');
    const productName = productNameElement.innerText;

    const modalResult = await openModal("Видалення", `Ви видаляєте позицію "${productName}" з кошику. Підтверджуєте?`, true);
    if (!modalResult) {
        return;
    }

    const spanElement = cdElement.parentNode.querySelector('[data-cart-detail-qnt]');
    const delta = -Number(spanElement.innerText);

    fetch(`/Shop/ModifyCart/${cdId}?delta=${delta}`, {
        method: 'PATCH',
    }).then(r => r.json()).then(j => {
        console.log(j);
        if (j.status < 300) {
            window.location.reload();
            return;
        }
        else {
            alert(j.message);
            return;
        }
    });
}

async function editCartBlur(e) {
    if (e.target.innerText === "") e.target.innerText = e.target.beforeEditing;

    if (e.target.beforeEditing != e.target.innerText) {
        const delta = Number(e.target.innerText) - Number(e.target.beforeEditing);
        const cdElement = e.target.closest('[data-cart-detail-qnt]');
        const cdId = cdElement.getAttribute('data-cart-detail-qnt');

        const productNameElement = e.target.closest('.card-detail')?.querySelector('#product-name');
        const productName = productNameElement.innerText;

        const modalResult = await openModal("Зміна", `Ви змінюєте кількість замовлення "${productName}" з ${e.target.beforeEditing} до ${e.target.innerText} шт. Підтверджуєте?`, true);
        if (!modalResult) {
            e.target.innerText = e.target.beforeEditing;
            return;
        }

        fetch(`/Shop/ModifyCart/${cdId}?delta=${delta}`, {
            method: 'PATCH',
        }).then(r => r.json()).then(j => {
            console.log(j);
            if (j.status < 300) {
                window.location.reload();
                return;
            }
            else {
                alert(j.message);
                e.target.innerText = e.target.beforeEditing;
            }
        });
    }
}

function editCartFocus(e) {
    e.target.beforeEditing = e.target.innerText;
}

function editCartEdit(e) {
    if (!([8, 13, 37, 39, 46, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57].includes(e.keyCode))) {
        e.preventDefault();
        return true;
    }
    if (e.keyCode == 13) {
        e.target.blur();
    }
}


function decrementCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest('[data-cart-detail-dec]');
    const cdId = cdElement.getAttribute('data-cart-detail-dec');
    console.log("- " + cdId);
    fetch(`/Shop/ModifyCart/${cdId}?delta=-1`, {
        method: 'PATCH',
    }).then(r => r.json()).then(j => {
        console.log(j);
        if (j.status < 300) {
            location = location;
            return;
        }
        else {
            alert(j.message);
            return;
        }
    });
}

function incrementCartClick(e) {
    e.stopPropagation();
    const cdElement = e.target.closest('[data-cart-detail-inc]');
    const cdId = cdElement.getAttribute('data-cart-detail-inc');
    console.log("+ " + cdId);
    fetch(`/Shop/ModifyCart/${cdId}?delta=1`, {
        method: 'PATCH',
    }).then(r => r.json()).then(j => {
        console.log(j);
        if (j.status < 300) {
            window.location.reload();
            return;
        }
        else {
            alert(j.message);
            return;
        }
    });
}

function addCartClick(e) {
    e.stopPropagation();
    const cartElement = e.target.closest('[data-cart-product]');
    const productId = cartElement.getAttribute('data-cart-product');
    console.log(productId);

    fetch('/Shop/AddToCart/' + productId, {
        method: 'PUT',
    })
        .then(r => r.json())
        .then(async j => {
            console.log(j);

            if (j.status == 401) {
                await openCartModal('Помилка', 'Увійдіть до системи для замовлення товарів');
                return;
            }
            else if (j.status == 400) {
                await openCartModal('Помилка', 'Невірний формат ідентифікатора товару. Спробуйте ще раз.');
                return;
            }
            else if (j.status == 404) {
                await openCartModal('Помилка', 'Обраний товар не знайдено. Можливо, він більше не доступний.');
                return;
            }
            else if (j.status == 201) {
                await openCartModal('Успіх', 'Товар додано. Бажаєте перейти до свого кошику?', true);
                return;
            }
            else {
                await openCartModal('Помилка', 'Щось пішло не так!');
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

function openModal(title, message, isApproval = false) {
    return new Promise((resolve) => {
        const confirmButton = isApproval ? `<button type="button" class="btn btn-primary" id="btn-approve">Так</button>` : '';
        const modalHTML = `<div class="modal" id="cartModal" tabindex="-1">
                         <div class="modal-dialog">
                             <div class="modal-content">
                                 <div class="modal-header">
                                     <h5 class="modal-title">${title}</h5>
                                     <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                                 </div>
                                 <div class="modal-body" style="white-space: pre-line;">
                                     <p>${message}</p>
                                 </div>
                                 <div class="modal-footer">
                                     <button type="button" class="btn btn-secondary" data-bs-dismiss="modal" id="btn-decline">${isApproval ? "Ні" : "Закрити"}</button>
                                     ${confirmButton}
                                 </div>
                             </div>
                         </div>
                       </div>`;

        document.body.insertAdjacentHTML('beforeend', modalHTML);
        const modalElement = document.getElementById('cartModal');
        const modalInstance = new bootstrap.Modal(modalElement);
        modalInstance.show();

        document.getElementById('btn-approve')?.addEventListener('click', () => resolve(true));
        document.getElementById('btn-decline').addEventListener('click', () => resolve(false));

        modalElement.addEventListener('hidden.bs.modal', () => {
            modalElement.remove();
            resolve(false);
        });
    });
}

async function openCartModal(title, message, isApproval = false) {
    const result = await openModal(title, message, isApproval);
    if (result) {
        window.location = '/User/Cart';
    }
}