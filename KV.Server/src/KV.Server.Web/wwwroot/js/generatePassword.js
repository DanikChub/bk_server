const containsUppercase = (password) => {
    return /[A-Z]/.test(password);
}

const containsLowercase = (password) => {
    return /[a-z]/.test(password);
}

const containsNumber = (password) => {
    return /\d/.test(password);
}

const containsSpecialCharacter = (password) => {
    return /[!@#$%^&*()_\-+=<>?]/.test(password);
}

const getRandomCharacter = (characters) => {
    const randomIndex = Math.floor(Math.random() * characters.length);
    return characters[randomIndex];
}

const createPassword = (generateSet, config) => {
    let password = '';

    for (let i = 0; i < config.minLength; i++) {
        password += getRandomCharacter(generateSet)
    }
    return password;
}

const tryAgainGeneratePassword = (generateSet, config) => {
    const password = createPassword(generateSet, config);
    if (
        (!config.includeLowerCase || containsLowercase(password)) &&
        (!config.includeUpperCase || containsUppercase(password)) &&
        (!config.includeNumber || containsNumber(password)) &&
        (!config.includeSpecialCharacters || containsSpecialCharacter(password))
    ) {
        return password;
    } else {
        return tryAgainGeneratePassword(generateSet, config);
    }
}

function generatePassword() {
    const config = window.kV.authSettings.passwordRequirements;
    const uppercaseLetters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const lowercaseLetters = 'abcdefghijklmnopqrstuvwxyz';
    const numbers = '0123456789';
    const specialCharacters = '!@#$%^&*';
    let generateSet = ''

    if (config.includeLowerCase) {
        generateSet += lowercaseLetters
    }

    if (config.includeUpperCase) {
        generateSet += uppercaseLetters
    }

    if (config.includeNumber) {
        generateSet += numbers
    }

    if (config.includeSpecialCharacters) {
        generateSet += specialCharacters
    }

    const password = tryAgainGeneratePassword(generateSet, config);

    $("#user-password").val(password)
}

function toggleImg() {
    const isOpened = $(this).attr('src') === '/img/eye-opened.svg';

    const src = isOpened ? '/img/eye-closed.svg' : '/img/eye-opened.svg';
    const top = isOpened ? '39px' : '42px';
    const right = isOpened ? '11px' : '12px';
    const passwordType = isOpened ? 'password' : 'text';

    $(this).attr('src', src).css({
        'top': top,
        'right': right
    });

    $("#user-password").attr('type', passwordType);
}

async function getEmployeePassword() {
    var id = $(".password__get-employee-password").data('id');
    var passwordEl = $('#current-user-password');

    if (passwordEl.val().length == 0) {
        await kV.server.employees.employeeProfile.getEmployeePassword(id)
            .then((password) => passwordEl.val(password));
    }
    else {
        passwordEl.val("");
    }
}

async function getCustomerUserPassword() {
    var id = $(".password__get-customer-user-password").data('id');
    var passwordEl = $('#current-user-password');
    var hiddenPasswordPlaceholder = "-";

    if (passwordEl.text() == hiddenPasswordPlaceholder) {
        await kV.server.employees.customerUserProfiles.getUserPassword(id)
            .then((password) => passwordEl.text(password));
    } else {
        passwordEl.text(hiddenPasswordPlaceholder);
    }
}

$("#generate-password").on('click', generatePassword);
$(".password__hide-password").on('click', toggleImg);
$(".password__get-employee-password").on('click', getEmployeePassword);
$(".password__get-customer-user-password").on('click', getCustomerUserPassword);