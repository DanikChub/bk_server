class Table {
    constructor(config) {
        this.config = config;
        this.initialize();
        this.filterModal(config);
    }

    filterModal(config) {
        const filterButton = $('#table-filter');

        if (!filterButton || !config?.filterParams) return;

        const prefix = config.filterPrefix;

        var setFilter = function () {
            const result = config?.filterParams?.map((elem) => `
                const ${elem.field} = $('#${elem.field}').val();
            `);

            result.push(`const fields = "${config?.filterParams?.map((elem) => elem.field)}"`);

            return result.join(";")
        };

        var setFilterCount = function () {
            const filterCountElem = $('#filter_count');
            const searchString = document.location.search;
            let count = '';

            if (!searchString) {
                filterCountElem.html(count);
                return;
            }

            const fields = config?.filterParams?.map((el) => el.field);

            searchString
                .slice(1)
                .split('&')
                .forEach((el) => {
                    const elem = el.replace(`${prefix}.`, '').split("=");

                    if (fields.includes(elem[0]) && elem[1]) {
                        count = +count + 1;
                    }
                });

            filterCountElem.html(count)
        };

        setFilterCount();

        filterButton.click(async function () {
            const styles = `display:flex;flex-direction:column;justify-content:space-between;padding:24px;
                width:600px;background:white;position:fixed;top:calc(150px);
                left:calc(50% - 300px);z-index:1000;box-shadow: 0 0 0 50vmax rgba(0,0,0,.5);`;

            const fieldsList = [];

            for (let field of config?.filterParams) {
                if (field.type === 'text') {
                    const result = `<div style="display:flex;align-items: center;margin-top: 16px">
                            <label style="min-width: 150px;margin-right: 10px;" for="${field.field}">${field.name}</label>
                            <input type="text" id="${field.field}" name="${field.field}" class="form-control">
                        </div>`

                    fieldsList.push(result);
                }

                if (field.type === 'select') {
                    const optionsList = await field.getItems();

                    const result = `<div style="display:flex;align-items: center;margin-top: 16px">
                            <label style="min-width: 150px;margin-right: 10px;" for="${field.field}">${field.name}</label>
                            <select style="width:100%" class="select" name="${field.field}" id="${field.field}">
                                <option value=""></option>
                                ${optionsList?.map((option) => {
                        return `<option value="${option.value}">${option.text}</option>`
                    })}
                            </select>
                    </div>`

                    fieldsList.push(result);
                }

                if (field.type === 'date') {
                    const result = `<div style="display:flex;align-items: center;margin-top: 16px">
                            <label style="min-width: 150px;margin-right: 10px;" for="${field.field}">${field.name}</label>
                            <input type="date" class="form-control" id="${field.field}"></input>
                            </select>
                    </div>`

                    fieldsList.push(result);
                }

                if (field.type === 'number') {
                    const result = `<div style="display:flex;align-items: center;margin-top: 16px">
                            <label style="min-width: 150px;margin-right: 10px;" for="${field.field}">${field.name}</label>
                            <input type="number" class="form-control" id="${field.field}"></input>
                            </select>
                    </div>`

                    fieldsList.push(result);
                }

                if (field.type === 'checkbox') {
                    const result = `<div style="display:flex;align-items: center;margin-top: 16px">
                    <label style="min-width: 150px;margin-right: 10px;" for="${field.field}">${field.name}</label>
                    <input type="checkbox" id="${field.field}"></input>
                    </select>
            </div>`

                    fieldsList.push(result);
                }
            };

            $('body').append(`
                <div id="customModal" style="${styles}">
                    <h3>Фильтр</h3>
                    <div>
                       ${fieldsList.join('')}
                    </div>
                    <div style="display:flex;align-items:center;margin-top:28px;gap:16px;">
                        <button type="button" class="button button_green" onclick="applyFilter()">
                            Применить фильтр
                        </button>
                        <button type="button" class="button button_red" onclick="clearFilter()">
                            Очистить фильтр
                        </button>
                        <button type="button" class="button button_red" onclick="closeModal()">
                            Отменить
                        </button>
                    </div>

                    <div style="position: absolute; top: 10px; right: 10px;">
                        <button class="btn-close" onclick="closeModal()"></button>
                    </div>

                    <script>
                        function closeModal() {
                            $('#customModal').remove();
                        };
           
                        function applyFilter() {
                            ${setFilter()}

                            const queryString = fields.split(',')?.map((field) => {
                                return "${prefix}." + field + "=" + $("#" + field).val() 
                            });
   
                            function filterArray(str, arr) {
                                return arr.filter(item => item.includes(str));
                            };

                            const notRelativeSearchString = [];
                            document.location.search.slice(1).split('&').forEach((elem) => {
                                const key = elem.slice("${prefix}".length + 1).split('=')[0];
                                const isElem = filterArray(key, queryString)

                                if (!isElem.length) notRelativeSearchString.push(elem);
                            });

                            queryString.push(...notRelativeSearchString);
                            document.location.href = document.location.href.replace(document.location.search, "") + "?" + queryString.join("&");
                        };

                        function clearFilter() {
                            document.location.href = document.location.href.replace(document.location.search, "");
                        };
                    </script>
                </div >
            `);
        });
    }

    initialize() {
        this.createTable();
        this.reset();
        if (this.config.tableIdModalApiPath.modalPath) {
            if (this.config.tableIdModalApiPath.modalPath.create) {
                this.create();
            }

            if (this.config.tableIdModalApiPath.modalPath.update) {
                this.update();
            }
        }

        this.button();
    }

    getUrl(input, ajaxParams) {
        let urlParams = new URLSearchParams(window.location.search);

        this.queryString = [];

        this.config.URLSearchParams.forEach((searchParam) =>
            this.queryString.push({
                name: searchParam.name,
                value: urlParams.get(searchParam.value),
            })
        );

        this.queryString.push({ name: 'sorting', value: input.sorting });
        this.queryString.push({ name: 'skipCount', value: input.skipCount });
        this.queryString.push({
            name: 'maxResultCount',
            value: input.maxResultCount,
        });

        if (this.config.specialQueryParams) {
            this.config.specialQueryParams.forEach((specialParam) =>
                this.queryString.push({
                    name: specialParam.name,
                    value: specialParam.value,
                })
            );
        }

        const newUrl =
            abp.appPath +
            this.config.tableIdModalApiPath.apiPath +
            abp.utils.buildQueryString([...this.queryString]);
        return abp.ajax(
            $.extend(
                true,
                {
                    url: newUrl,
                    type: 'GET',
                },
                ajaxParams
            )
        );
    }

    button() {
        const btn = $('#CreateNewCustomerProfileButton');
        if (btn) {
            btn.click((e) => {
                e.preventDefault();
                this.createModal.open();
            });
        }
    }

    setColVisBtn() {
        const colVisBtn = $('.colvis-btn');

        if (colVisBtn) {
            colVisBtn.detach();

            $('#btn-container').prepend(colVisBtn);
        }
    }

    reset() {
        const reset = $('#reset');
        if (reset) {
            reset.click(() => {
                $('.table__select, .table__input').val(null);
                const url = new URL(location.href);
                url.searchParams.forEach((_, key) =>
                    url.searchParams.set(key, '')
                );
                location.href = url;
            });
        }
    }

    update() {
        this.updateModal = new abp.ModalManager(
            abp.appPath + this.config.tableIdModalApiPath.modalPath.update
        );

        this.updateModal.onResult((e) => {
            e.preventDefault();
            abp.notify.success('Обновление выполнено успешно');
            this.dataTable.ajax.reload();
        });
    }

    create() {
        this.createModal = new abp.ModalManager(
            abp.appPath + this.config.tableIdModalApiPath.modalPath.create
        );

        this.createModal.onResult((e) => {
            e.preventDefault();
            abp.notify.success('Добавление выполнено успешно');
            this.dataTable.ajax.reload();
        });
    }

    createTable() {
        const table = this;
        this.dataTable = $(table.config.tableIdModalApiPath.id).DataTable(
            abp.libs.datatables.normalizeConfiguration({
                ...table.config.abpConfiguration,
                ajax: abp.libs.datatables.createAjax(table.getUrl.bind(table)),
                columnDefs: this.config.columnsData,
                ordering: false,
                initComplete: function () {
                    this.api()
                        .columns()
                        .every(function (index) {
                            const filterInput =
                                table.config.defaultViewForFilterFields.bind(
                                    this
                                );
                            filterInput(index, table.config.URLSearchParams);
                        });

                    const totalRecordsCount = document.getElementById(
                        `${table.config.tableIdModalApiPath.id.slice(1)} - Count`
                    );

                    if (totalRecordsCount) {
                        totalRecordsCount.innerText = this.api().page.info()
                            .recordsTotal
                            ? `(${this.api().page.info().recordsTotal})`
                            : '(0)';
                    }

                    $('[data-toggle="tooltip"]').tooltip({
                        container: '.card',
                    });
                },
            })
        );
    }
}
