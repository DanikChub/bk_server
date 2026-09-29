(async function () {
    const inputs = document.querySelectorAll(".input")

    const getBirthdays = async (startDate, endDate) => {
        const res = await kV.server.employees.customerUserProfiles.getCustomerDateOfBirthdayUsers(startDate, endDate);
        return res;
    };

    const sortByDate = (a, b) => {
        const dateA = moment(a.dateOfBirthDay);
        const dateB = moment(b.dateOfBirthDay);

        const monthDayA = dateA.format('MM-DD');
        const monthDayB = dateB.format('MM-DD');

        if (monthDayA > monthDayB) return -1;
        if (monthDayA < monthDayB) return 1;
        return 0;
    }

    const article = document.querySelector(".birthday");

    const showBirthdays = async () => {
        const startDate = moment(inputs[0].value, "DD.MM.YYYY").format("MM-DD-YYYY")
        const endDate = moment(inputs[1].value, "DD.MM.YYYY").format("MM-DD-YYYY")

        article.innerHTML = ''

        const birthdays = await getBirthdays(startDate, endDate);
        birthdays.sort(sortByDate);

        let currentMonthDay = null;

        birthdays.forEach((birthday) => {
            console.log(birthday)
            const momentDate = moment(birthday.dateOfBirthDay);
            const monthDay = momentDate.format('MM-DD');

            if (monthDay !== currentMonthDay) {
                const title = document.createElement("h5");
                title.classList.add("birthday__title");
                const formattedDate = moment(`${monthDay}-${moment().format("YYYY")}`, 'MM-DD-YYYY').format('DD MMMM YYYY, dddd');
                title.textContent = formattedDate;
                article.appendChild(title);
                currentMonthDay = monthDay;
            }

            const section = document.createElement("section");
            section.classList.add("birthday__info");

            const date = document.createElement("p");
            date.textContent = moment(birthday.dateOfBirthDay).format("DD.MM.YYYY");

            const person = document.createElement("p");
            person.textContent = birthday.lastName + " " + birthday.firstName + " " + birthday.middleName;
            person.classList.add("birthday__person");

            const company = document.createElement("p");
            company.textContent = birthday.tenantProfileDisplayName;

            const button = document.createElement("button");
            button.classList.add("button");
            button.textContent = "Открыть";
            button.onclick = () => location.href = `/Customers/DetailsEmployee/${birthday.id}`

            section.appendChild(date);
            section.appendChild(person);
            section.appendChild(company);
            section.appendChild(button);

            article.appendChild(section);
        });
    }

    document.getElementById("show-birthday").onclick = await showBirthdays
})();