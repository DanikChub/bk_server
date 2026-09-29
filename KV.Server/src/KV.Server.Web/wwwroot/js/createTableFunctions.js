const DEBOUNCE_TIMEOUT = 2000;

async function getStatusesFromServer() {
    const host = window;
    const res = await host.kV.server.tickets.ticketStatus.getListStatuses();
    const newStatuses = res.map((status) => ({
        innerText: status.displayNameMany,
        value: status.id,
    }));

    return newStatuses;
}

async function getPackagesFromServer() {
    const host = window;
    const res =
        await host.kV.server.crudServicePackage.getListServicePacakage();
    const newServicePackages = res.map((servicePackage) => ({
        innerText: servicePackage.name,
        value: servicePackage.id,
    }));

    return newServicePackages;
}

async function getPopularTagsFromServer() {
    const host = window;
    const res = await host.kV.server.tags.tags.getPopularByTicketTag();
    const newTags = res.map((tag) => ({
        innerText: tag.name,
        value: tag.id,
    }));

    return newTags;
}

async function getTicketTypesFromServer() {
    const host = window;
    const res = await host.kV.server.tickets.ticketType.getTicketTypes();
    const newTicketTypes = res.map((type) => ({
        innerText: type.name,
        value: type.id,
    }));

    return newTicketTypes;
}

async function getTicketSectionsFromServer() {
    const host = window;
    const res = await host.kV.server.tickets.ticketSection.getTicketSections();
    const newTicketSections = res.map((section) => ({
        innerText: section.name,
        value: section.id,
    }));

    return newTicketSections;
}

async function getContractStatusesFromServer() {
    const host = window;
    const res =
        await host.kV.server.crudContractStatus.getContractStatusesList();
    const newStatuses = res.map((status) => ({
        innerText: status.title,
        value: status.id,
    }));

    return newStatuses;
}

function debounce(callee, timeoutMs) {
    return function (...args) {
        let previousCall = this.lastCall;

        this.lastCall = Date.now();

        if (previousCall && this.lastCall - previousCall <= timeoutMs) {
            clearTimeout(this.lastCallTimer);
        }

        this.lastCallTimer = setTimeout(() => callee(...args), timeoutMs);
    };
}

function stopPropagation(event) {
    event.stopPropagation();
}

const isInputEmpty = (input) => {
    return input.value.trim() === '' ? true : false;
};

const changeSearchIconVisibility = (input) => {
    if (isInputEmpty(input) && !isInputTypeDate(input)) {
        input.previousElementSibling.style.display = '';
        input.nextElementSibling.style.display = 'none';
    } else {
        input.previousElementSibling.style.display = 'none';
        input.nextElementSibling.style.display = '';
    }
};

const isInputTypeDate = (input) => {
    const type = input.target ? input.target.type : input.type;

    if (type === 'date') {
        return true;
    } else {
        return false;
    }
};

const onFocus = (e) => {
    e.target.previousElementSibling.style.display = 'none';

    if (isInputTypeDate(e)) {
        e.target.showPicker();
    }
};

const onBlur = (e) => {
    if (isInputEmpty(e.target) && !isInputTypeDate(e)) {
        e.target.previousElementSibling.style.display = '';
    }
};

const headers = [];

function initCompleteFunc() {
    const column = this;
    const input = column.header().querySelector('input');
    const inputId = $(input).attr('id');
    const id = inputId ? inputId.replace('_', '.') : '';
    const datalist = column.header().querySelector('datalist');
    const header = {
        input,
        type: datalist ? 'datalist' : 'input',
    };
    headers.push(header);

    const handleClick = (e) => {
        const input = e.target.previousElementSibling;
        if (!isInputEmpty(input)) {
            e.target.style.display = 'none';
            input.value = '';
            handleDebounce();
        }
    };

    const handleInputChange = () => {
        const currentUrl = new URL(location.href);
        headers.forEach((header) => {
            const inputId = $(header.input).attr('id');
            const id = inputId ? inputId.replace('_', '.') : '';
            switch (header.type) {
                case 'input':
                    currentUrl.searchParams.set(
                        id,
                        header.input === null || header.input.value === 'null'
                            ? ''
                            : header.input.value
                    );
                    break;
                case 'datalist':
                    if (
                        header.input !== null &&
                        header.input.value !== 'null'
                    ) {
                        const value = header.input.value;
                        const dataList = document.getElementById(
                            header.input.name
                        );
                        const option = dataList.querySelector(
                            `[value='${value}']`
                        );
                        currentUrl.searchParams.set(
                            id,
                            option ? option.dataset.value : ''
                        );
                    }
                    break;
                default:
                    break;
            }
            location.href = currentUrl;
        });
    };

    const handleDebounce = () => {
        debounce(handleInputChange, DEBOUNCE_TIMEOUT)();
    };

    const setInputTypeDefaultValue = () => {
        const value = new URL(location.href).searchParams.get(id);

        if (!datalist) {
            input.value = value;
        } else {
            for (let i = 0; i < datalist.children.length; i++) {
                if (value === datalist.children[i].getAttribute('data-value')) {
                    input.value = datalist.children[i].value;
                    return;
                }
            }
        }
    };

    const addInputEventListeners = () => {
        input.addEventListener('blur', onBlur);
        input.addEventListener('focus', onFocus);
        input.addEventListener('click', stopPropagation);
        input.addEventListener('keypress', stopPropagation);
        input.addEventListener('keyup', () => handleDebounce());
        input.addEventListener('change', () => handleDebounce());

        input.previousElementSibling.addEventListener('click', stopPropagation);
        input.nextElementSibling.addEventListener('click', handleClick);
        input.nextElementSibling.addEventListener('click', stopPropagation);
    };

    if (input) {
        addInputEventListeners();
        setInputTypeDefaultValue();
        changeSearchIconVisibility(input);

        input.addEventListener('keyup', () =>
            changeSearchIconVisibility(input)
        );
    }
}

function createColumnHeader(headerType, headerInfo) {
    const tableField = document.createElement('div');
    tableField.classList.add('table__field');

    const fieldName = document.createElement('label');
    fieldName.innerText = headerInfo.title;
    fieldName.for = headerInfo.id;

    const inputWrapper = document.createElement('div');
    inputWrapper.style.position = 'relative';

    const icon = document.createElement('span');
    icon.classList.add('fa', 'fa-search', 'icon');
    icon.style.cursor = 'default';

    const cleanFilter = document.createElement('img');
    cleanFilter.classList.add('icon_clean-filter');
    cleanFilter.src = '/assets/table/cleanFilter.svg';

    const input = document.createElement('input');
    input.classList.add(
        'input_small',
        'form-control',
        'input_small_with-icons'
    );

    inputWrapper.appendChild(icon);
    inputWrapper.appendChild(input);
    inputWrapper.appendChild(cleanFilter);
    tableField.appendChild(fieldName);
    tableField.appendChild(inputWrapper);

    if (headerType.main === 'select') {
        input.setAttribute('list', headerInfo.name);
        input.setAttribute('name', headerInfo.name);
        input.id = headerInfo.id;
        input.value = 'Все';

        const selectList = document.createElement('datalist');
        selectList.id = headerInfo.name;

        for (let i = 0; i < headerInfo.optionsToChoose.length; i++) {
            const option = document.createElement('option');
            option.value = headerInfo.optionsToChoose[i].innerText;
            option.dataset.value = headerInfo.optionsToChoose[i].value;
            selectList.appendChild(option);
        }

        if (headerType.sub === 'status') {
            selectList.classList.add('table__select_status');
        }

        inputWrapper.appendChild(selectList);

        return tableField.outerHTML;
    }

    if (headerType.main === 'input') {
        input.id = headerInfo.id;
        input.name = headerInfo.name;

        if (headerType.sub === 'date') {
            input.type = 'date';
        }

        return tableField.outerHTML;
    }

    return;
}

function createRedirectButton(href) {
    return `
        <a style="position: relative;" href=${href}>
            <img style="position: relative; left: -7px;" src='/assets/table/edit.svg' alt='redirect'></img>
        </a>
    `;
}

function createColumnField(title, id, name, optionsToChoose) {
    const obj = {
        title,
        id,
        name,
    };

    if (optionsToChoose) {
        obj.optionsToChoose = optionsToChoose;
    }

    return obj;
}


function setTableTitle(title, width) {
    return `
            <div class="table__field" style="width: ${width}px">
                <label style="padding-top: 22px">${title}</label>
            </div>
    `
};
