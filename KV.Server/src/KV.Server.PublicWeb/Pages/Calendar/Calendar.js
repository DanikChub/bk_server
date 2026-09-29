async function getEvents(startPeriod, endPeriod) {
    const host = window;
    const res = await host.kV.server.event.getListEventsByPeriod(
        startPeriod,
        endPeriod
    );
    return res;
}

const getCalendarDays = async (startDate, endDate) => {
    const host = window;
    const res = await host.kV.server.workCalendar.getCountDays(
        startDate,
        endDate
    );
    return res;
};

const renderEventList = (events) => {
    const eventList = document.querySelector('.task-list');
    eventList.innerText = '';
    events.length !== 0
        ? events.forEach((event) => {
              const eventItem = document.createElement('div');
              eventItem.classList.add('task-list__item');

              const eventStatus = document.createElement('input');
              eventStatus.type = 'checkbox';
              eventStatus.classList.add('task-list__checkbox');

              const eventTitle = document.createElement('p');
              eventTitle.innerText = event.title;

              eventItem.appendChild(eventStatus);
              eventItem.appendChild(eventTitle);
              eventList.appendChild(eventItem);
          })
        : (eventList.innerHTML = '<p>Событий нет</p>');
};

const getEventsForToday = (events, currentDate) =>
    events.filter((event) => {
        const eventDate = event.eventDate;
        const momentEventDate = moment(eventDate).format('YYYY-MM-DD');
        const momentCurrentDate = moment(currentDate).format('YYYY-MM-DD');
        return moment(momentEventDate).isSame(momentCurrentDate);
    });

const handleClick = (e, events, currentDate) => {
    const days = document.querySelectorAll('.day');
    days.forEach((day) => day.classList.remove('day_selected'));
    e.currentTarget.classList.add('day_selected');

    const eventsForToday = getEventsForToday(events, currentDate);
    renderEventList(eventsForToday);

    e.stopPropagation();
};

const renderCalendar = async (year, month) => {
    const calendar = [];
    const currentYearMonth = moment(`${year}-${month}`, 'YYYY-MM');
    const today = moment().format('D');
    const daysInMonth = currentYearMonth.daysInMonth();
    const firstDayOfMonth = currentYearMonth.date(1);
    const startingDayOfWeek = firstDayOfMonth.isoWeekday();
    const daysFromPreviousMonth = startingDayOfWeek - 1;

    const startOfMonth = currentYearMonth.startOf('month').format('YYYY-MM-DD');
    const endOfMonth = currentYearMonth.endOf('month').format('YYYY-MM-DD');

    const events = await getEvents(startOfMonth, endOfMonth);

    for (let i = 0; i < daysFromPreviousMonth; i++) {
        const dayElement = document.createElement('div');
        dayElement.classList.add('day', 'day_invalid', 'day_bordered');

        const dayTitle = document.createElement('p');
        dayTitle.classList.add('day__title');

        const prevMonthDay = moment(firstDayOfMonth).subtract(
            daysFromPreviousMonth - i,
            'days'
        );
        dayTitle.textContent = prevMonthDay.format('D');

        dayElement.appendChild(dayTitle);
        calendar.push(dayElement);
    }

    for (let i = 0; i < daysInMonth; i++) {
        const currentDate = moment(firstDayOfMonth).date(i + 1);

        const dayElement = document.createElement('div');
        dayElement.classList.add('day', 'day_bordered');
        dayElement.addEventListener('click', (e) =>
            handleClick(e, events, currentDate)
        );

        const dayTitle = document.createElement('p');
        dayTitle.classList.add('day__title');
        dayTitle.textContent = currentDate.format('D');

        const dayWrapper = document.createElement('section');
        dayWrapper.classList.add('day__section');

        dayWrapper.appendChild(dayTitle);

        dayElement.appendChild(dayWrapper);

        const eventWrapper = document.createElement('section');
        eventWrapper.classList.add('day__event-wrapper');

        const eventsForCurrentDay = getEventsForToday(events, currentDate);

        eventsForCurrentDay.forEach((event, index) => {
            if (index < 4) {
                const taskCheckbox = document.createElement('input');
                taskCheckbox.type = 'checkbox';
                taskCheckbox.classList.add('day__task-is-done');
                eventWrapper.appendChild(taskCheckbox);
            }
        });

        if (eventsForCurrentDay.length !== 0) {
            const dayWithEvent = document.createElement('p');
            dayWithEvent.classList.add('day__event-signal');
            dayWrapper.appendChild(dayWithEvent);
        }

        dayElement.appendChild(eventWrapper);

        if (currentDate.isoWeekday() === 6 || currentDate.isoWeekday() === 7) {
            dayElement.classList.add('day_weekend');
        }

        if (
            currentDate.format('D') === today &&
            currentDate.month() === moment().month()
        ) {
            dayTitle.classList.add('day_today');
            dayElement.classList.add('day_selected');
        }

        calendar.push(dayElement);
    }

    const daysFromNextMonth = 7 - (calendar.length % 7);

    for (let i = 0; i < daysFromNextMonth; i++) {
        const invalidNextMonthDay = document.createElement('div');
        const invalidNextMonthDayTitle = document.createElement('p');

        invalidNextMonthDayTitle.textContent = i + 1;
        invalidNextMonthDay.appendChild(invalidNextMonthDayTitle);
        invalidNextMonthDay.classList.add('day', 'day_invalid', 'day_bordered');
        invalidNextMonthDayTitle.classList.add('day__title');
        calendar.push(invalidNextMonthDay);
    }

    const eventsForCurrentDay = getEventsForToday(
        events,
        moment().format('YYYY-MM-DD')
    );
    renderEventList(eventsForCurrentDay);

    const calendarContent = document.querySelector('.calendar__content');
    calendarContent.innerHTML = '';
    calendar.forEach((day) => calendarContent.appendChild(day));
};

async function onChange(e) {
    const [year, month] = e.target.value.split('-');
    const startDate = moment()
        .clone()
        .set({ year: year, month: Number(month) - 1, date: 1 });
    const endDate = startDate.clone().endOf('month');
    const calendarDays = await getCalendarDays(
        startDate.toDate(),
        endDate.toDate()
    );
    renderCalendar(year, month);

    $('#CountDays').text(calendarDays.countDays);
    $('#CountWorkdays').text(calendarDays.countWorkdays);
    $('#CountWeekends').text(calendarDays.countWeekends);
    $('#CountHolidays').text(calendarDays.countHolidays);
}

const moveToCurrentDay = () => {
    renderCalendar(moment().year(), moment().month() + 1);
    const datePicker = document.querySelector('.menu__date-picker');
    datePicker.value = moment().format('YYYY-MM');
};

renderCalendar(moment().year(), moment().month() + 1);
const datePicker = document.querySelector('.menu__date-picker');
datePicker.onchange = onChange;
datePicker.value = moment().format('YYYY-MM');

const todayButton = document.querySelector('.menu__today');
todayButton.onclick = moveToCurrentDay;
