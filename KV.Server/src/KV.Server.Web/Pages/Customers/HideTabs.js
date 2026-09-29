(() => {
    const ATTRIBUTE = 'data-tab-category';
    const ACTIVE_CLASS = 'tub-item_active';

    const buttonsList = document.querySelector('.tubs-container-list');
    const buttons = document.querySelectorAll(
        '.tubs-container-list button'
    );
    const tabList = document.querySelectorAll(
        '.customers__tab-content div[data-tab-category]'
    );

    const currentTab = sessionStorage.getItem('currentTab');
    const activeIndex = [...buttons].findIndex(
        (btn) => btn.innerText === currentTab
    );

    const activeTab = activeIndex ? tabList[activeIndex] : tabList[0];

    tabList.forEach((element) => {
        if (element !== activeTab) {
            element.classList.add('hide');
        }
    });

    let selectedButton = buttons[activeIndex > 0 ? activeIndex : 0];
    selectedButton.classList.add(ACTIVE_CLASS);

    buttonsList.addEventListener('click', (event) => {
        const target = event.target;
        changeSelectedTab(target);
    });

    var changeSelectedTab = function (target) {
        if (target.tagName !== 'BUTTON') return;

        if (selectedButton) {
            sessionStorage.setItem('currentTab', target.innerText);
            selectedButton.classList.remove(ACTIVE_CLASS);

            tabList.forEach((element) => {
                const elemAtrData = element.getAttribute(ATTRIBUTE);
                const buttonData = target.getAttribute(ATTRIBUTE);
                if (elemAtrData !== buttonData) {
                    element.classList.add('hide');
                } else {
                    element.classList.remove('hide');

                    if (target.dataset.id) {
                        $(`#${target.dataset.id}`)
                            .DataTable()
                            .columns.adjust()
                            .draw();
                    }
                }
            });
        }

        selectedButton = target;
        selectedButton.classList.add(ACTIVE_CLASS);
    }

    $(document).ready(function () {
        var target = $(location).attr('hash');
        if (target[0] === "#") {
            var button = $(target);
            setTimeout(() => changeSelectedTab(button[0]), 500);
        };
    });
})();
