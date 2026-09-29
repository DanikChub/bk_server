const SLIDER_SERVICES_TYPE = "Services"
const SLIDER_NEWS_TYPE = "News"

$(async function () {
    const getStoriesSlides = async (slidesType) => {
        const res = await kV.server.storiesClient.getListActiveStoryByGroupName(slidesType)
        return res;
    }

    const getStories = async (storyId) => {
        const res = await kV.server.storiesManagement.getSlides(storyId)
        return res;
    }

    const generateSliders = async (slidesType) => {

        const storiesList = await getStoriesSlides(slidesType);

        const sliders = []

        for (const story of storiesList) {
            const sliderImg = await getStories(story.id);
            const slides = sliderImg.map(slider => ({
                id: slider.id,
                type: "photo",
                length: 3,
                imageUrl: slider.imageUrl ?? '/img/sliders/main.svg'
            }));

            sliders.push({
                items: [{
                    id: story.id,
                    coverImageUrl: story.coverImageUrl,
                    slides,
                }]
            });
        }

        return {
            sliders
        };
    }

    const newsStoriesList = await generateSliders(SLIDER_NEWS_TYPE)
    const servicesStoriesList = await generateSliders(SLIDER_SERVICES_TYPE)

    const sliderStorisWrapper = document.querySelector('#slider1');
    const sliderWrapper = document.querySelector('#slider2');

    let index = 0;
    servicesStoriesList.sliders.forEach((el) => {
        const div = document.createElement('div');
        const storisElement = document.createElement('div');

        div.setAttribute('class', 'keen-slider__slide');

        storisElement.setAttribute('id', `stories${index}`);
        storisElement.setAttribute('class', 'storis-item');

        div.append(storisElement);

        sliderWrapper.append(div);

        const options = {
            backNative: true,
            previousTap: true,
            skin: 'Default',
            autoFullScreen: false,
            avatars: true,
            paginationArrows: false,
            list: false,
            cubeEffect: true,
            localStorage: true,
            stories: el,
        };

        new CustomStories(`#stories${index}`, options);
        index++;
    });

    newsStoriesList.sliders.forEach((el) => {
        const div = document.createElement('div');
        const storisElement = document.createElement('div');

        div.setAttribute('class', 'keen-slider__slide');

        storisElement.setAttribute('id', `stories${index}`);
        storisElement.setAttribute('class', 'storis-item');

        div.append(storisElement);

        sliderStorisWrapper.append(div);

        const options = {
            backNative: true,
            previousTap: true,
            skin: 'Default',
            autoFullScreen: false,
            avatars: true,
            paginationArrows: false,
            list: false,
            cubeEffect: true,
            localStorage: true,
            stories: el,
        };

        new CustomStories(`#stories${index}`, options);
        index++;
    });

    const sliderOptions = {
        drag: true,
        loop: true,
    };

    const delay1 =
        Math.ceil((3000 + Math.floor(Math.random() * 8000)) / 1000) * 1000;
    const delay2 =
        Math.ceil((2000 + Math.floor(Math.random() * 7000)) / 1000) * 1000;

    const sliderAutoPlayFn = (delay) => {
        return [
            (slider) => {
                let timeout;
                let mouseOver = false;
                function clearNextTimeout() {
                    clearTimeout(timeout);
                }

                function nextTimeout() {
                    clearTimeout(timeout);
                    if (mouseOver) return;
                    timeout = setTimeout(() => {
                        slider.next();
                    }, delay);
                }
                slider.on('created', () => {
                    slider.container.addEventListener('mouseover', () => {
                        mouseOver = true;
                        clearNextTimeout();
                    });

                    slider.container.addEventListener('mouseout', () => {
                        mouseOver = false;
                        nextTimeout();
                    });

                    nextTimeout();
                });
                slider.on('dragStarted', clearNextTimeout);
                slider.on('animationEnded', nextTimeout);
                slider.on('updated', nextTimeout);
            },
        ];
    };

    const createButtons = (dataSlider, slider) => {
        const wrapper = document.querySelector(
            `.change-slide[data-slider=${dataSlider}]`
        );
        for (let i = 0; i < slider.slides.length; i++) {
            const btn = document.createElement('button');
            if (i === 0) {
                btn.classList.add(
                    'change-slide__button',
                    'change-slide__button_active'
                );
            } else {
                btn.classList.add('change-slide__button');
            }
            btn.dataset.slider = dataSlider;
            wrapper.appendChild(btn);
        }
    };

    const updateClasses = (dotButtons, slider) => {
        const slide = slider.track.details.rel;
        [...dotButtons].forEach((dot, idx) => {
            idx === slide
                ? dot.classList.add('change-slide__button_active')
                : dot.classList.remove('change-slide__button_active');
        });
    };

    const changeSlideOnClick = (buttons, slider) => {
        [...buttons].forEach((btn, idx) =>
            btn.addEventListener('click', () => {
                slider.moveToIdx(idx);
            })
        );
    };

    const createSlider = (sliderId, delay) => {
        const slider = new KeenSlider(
            '#' + sliderId,
            sliderOptions,
            sliderAutoPlayFn(delay)
        );
        const leftArrow = document.querySelector(
            `.arrow-button_left[data-slider=${sliderId}]`
        );
        const rightArrow = document.querySelector(
            `.arrow-button_right[data-slider=${sliderId}]`
        );
        leftArrow.addEventListener('click', () => slider.prev());
        rightArrow.addEventListener('click', () => slider.next());
        createButtons(sliderId, slider);
        const buttons = document.querySelectorAll(
            `.change-slide__button[data-slider=${sliderId}]`
        );
        changeSlideOnClick(buttons, slider);

        slider.on('slideChanged', () => {
            updateClasses(buttons, slider);
        });
    };

    createSlider('slider1', delay1);
    createSlider('slider2', delay2);
});

