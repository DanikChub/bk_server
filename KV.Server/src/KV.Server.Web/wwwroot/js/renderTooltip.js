const renderTooltip = (data) => {
    const placement = 'data-placement="top"';
    if (data && (typeof data === 'string' && data.trim() !== '')) {
        const str = data.toString();
        return `<div data-toggle="tooltip" ${placement} title='${str}'>${data}</div>`
    } else {
        return `<div data-toggle="tooltip" ${placement} title='Отсутствует'>Отсутствует</div>`
    }
};

const renderDateWithTooltip = (data) => {
    if (data) {
        const str = `${moment(data).format("DD.MM.YYYY")}`
        return `<div data-toggle="tooltip" data-placement="top" title='${str}'>${str}</div>`
    } else {
        return `<div data-toggle="tooltip" data-placement="top" title='Отсутствует'>Отсутствует</div>`
    }
}

const renderTag = (data) => {
    return data
        ? `
            <p class="table__tag" data-toggle="tooltip" data-placement="top" title='${data}'>${data}</p>
    ` : `<p class="table__tag" data-toggle="tooltip" data-placement="top" title='Отсутствует'>Отсутствует</p>`
}

const renderVolga = (data) => {
    return data === null
        ? '<div data-toggle="tooltip" data-placement="top" title="Консалтинг-Волга">Консалтинг-Волга</div>'
        : `<div data-toggle="tooltip" data-placement="top" title='${data}'>${data}</div>`;
}

const getPackageColor = (package) => {
    let backgroundColor;
    const packageToLowerCase = package.toLowerCase().trim();
    switch (packageToLowerCase) {
        case "консалтинг":
            backgroundColor = "#78B7D9"
            break;
        case "it":
            backgroundColor = "#7AE68D"
            break;
        case "сайт":
            backgroundColor = "#FFD887"
            break
        default:
            backgroundColor = "#DDE4E8"
            break;
    }
    return backgroundColor;
}

const renderPackageTooltip = (data) => {
    const severalPackages = data.split(',')
    let result = ''
    severalPackages.map(package => {
        if (package.trim() === '') {
            result += `<div data-toggle="tooltip" class="package" style="background-color:${getPackageColor(package)}" data-placement="top" title="Отсутствует">Отсутствует</div>`
        } else result += `<div data-toggle="tooltip" class="package" style="background-color:${getPackageColor(package)}" data-placement="top" title="${package}">${package}</div>`
    })
    return result;
};
