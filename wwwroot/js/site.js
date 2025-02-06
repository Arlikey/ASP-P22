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