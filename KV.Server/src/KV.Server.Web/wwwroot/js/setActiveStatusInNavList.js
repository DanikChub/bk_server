function sortByStatus(_, id) {
    if (id) {
        const currentUrl = new URL(location.href);
        const paramName = "Search.UpperTicketStatusId";
        currentUrl.searchParams.set(paramName, id);
        location.href = currentUrl
    } else {
        const url = new URL(location.href)
        url.searchParams.delete("Search.UpperTicketStatusId")
        location.href = url
    }
}
const list = document.querySelectorAll(".tub-item");

const setActiveTab = (tab) => {
    list.forEach(item => item.classList.remove("tub-item_active"));
    tab.classList.add("tub-item_active");
};

const updateActiveTab = () => {
    const searchParams = new URL(location.href).searchParams;
    const upperTicketStatusId = searchParams.get("Search.UpperTicketStatusId");

    list.forEach(item => item.classList.remove("tub-item_active"));

    const link = [...list].find(link => link.id === upperTicketStatusId);
    if (link) {
        link.classList.add("tub-item_active");
    } else {
        list[5].classList.add("tub-item_active");
    }
};

const onClick = (event) => {
    const clickedTab = event.target.closest("li");
    setActiveTab(clickedTab);
    updateActiveTab();
};

list.forEach(link => link.onclick = onClick);

updateActiveTab();