(async () => {
    const host = window;

    const postsContainer = document.querySelector('.main-page__library');
    const newsContainer = document.querySelector('.posts-container');

    const LIBRARY_HOST = newsContainer.getAttribute('data-labraby');

    const getNews = async (page, count) => {
        const news = await host.kV.server.posts
            .getNews(page, count)
            .then((res) => res.items);

        return news;
    };

    const getPosts = async (page, count) => {
        const posts = await host.kV.server.posts
            .getArticles(page, count)
            .then((res) => res.items);

        return posts;
    };

    const news = await getNews(0, 5);
    const posts = await getPosts(0, 5);

    news.forEach((elem) => {
        const result = `<a style="color: rgb(103, 106, 108);" href="${LIBRARY_HOST}news/${elem.attributes.slug}" target="_blank">
                            <li>${elem.attributes.title}</li>
                        </a>`;

        newsContainer.insertAdjacentHTML('beforeend', result);
    });

    posts.forEach((elem) => {
        let imgUrl;
        switch (elem.categoryName) {
            case '':
                imgUrl = '/img/question-icons/library1.svg';
                break;
            case '':
                imgUrl = '/img/question-icons/library3.svg';
                break;
            case '':
                imgUrl = '/img/question-icons/library4.svg';
                break;
            case '':
                imgUrl = '/img/question-icons/library5.svg';
                break;
            default:
                imgUrl = '/img/question-icons/library.svg';
                break;
        }
        const result = `<a style="color: rgb(103, 106, 108);" href="${LIBRARY_HOST}posts/${elem?.slug}" target="_blank">
                            <article class="main-page__advice">
                                <img src="${imgUrl}" alt="icon">
                                <p>
                                    ${elem.title}
                                </p>
                            </article>
                        </a>`;

        postsContainer.insertAdjacentHTML('beforeend', result);
    });
})();
