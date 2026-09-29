(() => {
    $('#delete-customer').on(
        'click',
        function (e) {
            const id = $(this).data('id');
            e.preventDefault();

            abp.message.confirm("", "Вы действительно хотите удалить клиента?", async (param) => {
                if (param) {
                    kV.server.crudCustomer.delete(id)
                        .done(() => {
                            abp.message.success("", "Клиент удален")
                                .done(() => { document.location.href = '/Customers'; });
                        });
                }
            })
        }
    );
})();