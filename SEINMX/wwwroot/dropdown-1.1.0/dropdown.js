import {
    computePosition,
    offset,
    flip,
    shift,
    autoUpdate
} from 'https://cdn.jsdelivr.net/npm/@floating-ui/dom@1.6.3/+esm';
import {InMemoryDataProvider} from "./dataProviders.js";


/*const getCookie = (name) => {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) {
        return parts.pop().split(';').shift();
    }
}*/

/**
 * @typedef {Object} DropdownOption
 * @property {string} display
 * @property {string} value
 */

/**
 * @typedef {Object} DropdownFetchParameters
 * @property {string} search - The search string to fetch data accordingly.
 * @property {Array<string>} selectedOptions - The currently selected options in the dropdown.
 */

export class CbDropdown extends HTMLElement {
    /**
     * @template T
     * @template F
     * @typedef {Object} DataProvider
     * @property {function(DropdownFetchParameters): Promise<T>|T} open - A function that fetches dropdown options asynchronously.
     * @property {function(DropdownFetchParameters): Promise<T>|T} search - A function that fetches dropdown options asynchronously.
     * @property {function(DropdownFetchParameters): Promise<T>|T} scroll - A function that fetches dropdown options asynchronously.
     * @property {function(string): Promise<DropdownOption>|DropdownOption|undefined} find - A function that finds one option by value.
     * @property {boolean} moreToLoad - A getter that indicates if there is more data to fetch.
     */

    /**
     * @template T
     * @typedef {Object} DropdownRenderParameters
     * @property {string} search - The search string to fetch data accordingly.
     * @property {Array<string>} selectedOptions - The currently selected options in the dropdown.
     * @param {T} data - The data returned by dataProvider.
     * @param {Element} tooltip - The tooltip element where the option elements should be inserted as children.
     */

    /**
     * @template T
     * @typedef {function(parameters): void} DropdownRenderFunction
     * @async
     * @description Function to render the dropdown options.
     * This function should insert the option elements as children of the tooltip element.
     * @param {DropdownRenderParameters} parameters
     *
     */

    /**
     * @type {ElementInternals}
     */
    #internals

    /**
     * The elements contained within the container.
     * @type {Object}
     * @property {Element} wrapper - The wrapper element.
     * @property {Element} button - The button element.
     * @property {Element} searchBox - The search box element.
     * @property {Element} input - The hidden input element.
     * @property {Element} placeholder - The placeholder element.
     * @property {Element} tooltip - The tooltip element.
     * @property {Element[]} options - The options within the tooltip.
     */
    #elements = {};

    /**
     * Currently selected dropdown option values(s)
     * @type {(Element[])}
     */
    #selectedElements = [];

    /**
     * Currently selected dropdown option values(s)
     * @type {(string[])}
     */
    #_selected = [];

    get #selected() {
        return this.#_selected;
    }

    /**
     *
     * @param {string[]} x
     */
    set #selected(x) {
        this.#_selected = x;

        if (x.length === 1) {
            this.#internals.setFormValue(x[0]);
        } else {
            this.#internals.setFormValue(undefined);
        }
    }

    get selected() {
        return this.#_selected;
    }

    /**
     * Cleanup function for the floating UI autoUpdate feature.
     * @type {(null|function(): null)}
     */
    #cleanup = null;

    /**
     * Indicates whether the user has edited the search input of the dropdown.
     * @type {boolean}
     */
    #dirty = false;

    /**
     * The data returned by dataProvider.
     * @type {T}
     */
    data;

    /**
     * @type {'es'|'en'}
     */
    lang = 'en';

    placeholder = '';
    previousPlaceholder = '';


    constructor() {
        super();


        //this.#setLanguage((getCookie("Language") ?? "").split("-")[0]);
        this.#setLanguage("es");

        const shadow = this.attachShadow({mode: "open"});

        const linkElement = document.createElement('link');
        linkElement.setAttribute('rel', 'stylesheet');
        linkElement.setAttribute('href', '/dropdown-1.1.0/dropdown.css');

        const linkElement2 = document.createElement('link');
        linkElement2.setAttribute('rel', 'stylesheet');
        linkElement2.setAttribute('href', '/lib/bootstrap/dist/css/bootstrap.min.css');

        const wrapper = document.createElement('div');
        wrapper.setAttribute('class', 'cb-dropdown');

        const button = document.createElement('button');
        button.setAttribute('class', 'form-select');
        button.setAttribute('role', 'combobox');

        const searchBox = document.createElement('input');
        searchBox.setAttribute('autocomplete', 'off');
        searchBox.setAttribute('autocorrect', 'off');
        searchBox.setAttribute('spellcheck', 'false');
        searchBox.setAttribute('tabindex', '0');
        searchBox.setAttribute('type', 'text');
        searchBox.setAttribute('aria-autocomplete', 'list');
        searchBox.setAttribute('aria-expanded', 'false');
        searchBox.setAttribute('aria-controls', 'listbox');
        searchBox.setAttribute('aria-haspopup', 'true');
        searchBox.setAttribute('role', 'combobox');
        searchBox.setAttribute('value', '');

        const span = document.createElement('span');
        span.setAttribute('style', 'position: absolute; overflow: hidden; text-overflow: ""; white-space: nowrap; width: 100%; padding-right: 40px; display: inline-block;');
        span.textContent = this.placeholder;

        const ul = document.createElement('ul');
        ul.setAttribute('role', 'listbox');

        shadow.appendChild(linkElement);
        shadow.appendChild(linkElement2);
        button.appendChild(searchBox);
        button.appendChild(span);
        wrapper.appendChild(button);
        wrapper.appendChild(ul);
        shadow.appendChild(wrapper);

        this.#elements.wrapper = wrapper;
        this.#elements.button = button;
        this.#elements.searchBox = searchBox;
        this.#elements.placeholder = span;
        this.#elements.tooltip = ul;
        this.#elements.options = [];

        this.#elements.searchBox.addEventListener('keydown', this.#handleKeyPress);
        this.#elements.searchBox.addEventListener('input', this.#handleSearchUpdate);
        this.#elements.button.addEventListener('pointerup', this.#showDropdown);
        this.#elements.button.addEventListener('focus', this.#focusInput);
        this.#elements.tooltip.addEventListener('wheel', this.#loadMoreData);

        this.#internals = this.attachInternals();

        this.addEventListener("pointerup", ({target, x, y}) => {
            const relatedTarget = document.elementFromPoint(x, y);

            if (target === this &&
                new Set(this.#internals.labels).has(relatedTarget) &&
                this.#elements.searchBox.getAttribute('aria-expanded') === 'false') {
                this.#elements.button.click();
                this.#elements.button.focus();
            }
        });
    }

    static get formAssociated() {
        return true;
    }

    static #languagePlaceholders = {
        'es': 'Selecciona...',
        'en': 'Choose...',
    };

    #setLanguage = (lang) => {
        this.lang = CbDropdown.#languagePlaceholders[lang] ? lang : 'en';
        this.placeholder = CbDropdown.#languagePlaceholders[this.lang];
    }

    static get observedAttributes() {
        return ["placeholder", "onchange", "options", "selected"];
    }

    #onChangeFunction;

    async attributeChangedCallback(name, oldValue, newValue) {
        switch (name) {
            case 'placeholder':
                this.#elements.placeholder.name = newValue;
                break;
            case 'onchange':
                if (this.#onChangeFunction != null) {
                    this.removeEventListener('change', this.#onChangeFunction);
                }
                this.#onChangeFunction = () => eval(newValue);
                this.addEventListener('change', this.#onChangeFunction);
                break;
            case 'options':
                try {
                    this.dataProvider = new InMemoryDataProvider(JSON.parse(newValue))
                } catch {
                    this.dataProvider = new InMemoryDataProvider([])
                }
                break;
            case 'selected':
                this.#elements.placeholder.textContent = '';

                const opt = await this.dataProvider.find(newValue);
                if (opt != null) {
                    this.#elements.placeholder.textContent = opt.display;
                    this.#selected = [opt.value];
                } else {
                    this.#elements.placeholder.textContent = this.placeholder;
                    this.#selected = [];
                }

                break;
        }
    }

    fetching = false;

    /**
     * Asynchronously loads more data when reaching the end of the dropdown.
     *
     * @param {Event} event - The scroll event triggering the function.
     *
     * @returns {Promise} A promise that resolves once the data is rendered.
     */
    #loadMoreData = async (event) => {
        if (!this.dataProvider.moreToLoad || this.fetching) {
            return
        }

        this.fetching = true

        try {
            const {scrollTop, scrollHeight, clientHeight} = event.target;

            if (scrollTop + clientHeight >= scrollHeight - 400) {
                this.data = await this.dataProvider.scroll(this.#fetchParameters);

                if (this.currentOptionIndex != null) {
                    this.#elements.options[this.currentOptionIndex].classList.remove('cb-dropdown-focused');
                    this.currentOptionIndex = null;
                }

                this.#render(this.#renderParameters);
            }
        } catch (e) {
            console.error(e);
        }

        this.fetching = false
    }

    /**
     * Represents a getter method which provides the currently selected option's value and the current search value.
     * @returns {DropdownFetchParameters} An object that holds the selected options and input value from the search field.
     */
    get #fetchParameters() {
        return {
            selectedOptions: this.#selected,
            search: this.#elements.searchBox.value
        };
    }

    /**
     * Represents a getter method which provides the currently selected option's value and the current search value.
     * @returns {DropdownRenderParameters} An object that holds the selected options and input value from the search field.
     */
    get #renderParameters() {
        return {
            selectedOptions: this.#selected,
            search: this.#elements.searchBox.value,
            data: this.data,
            tooltip: this.#elements.tooltip
        };
    }

    #focusInput = () => {
        this.#elements.searchBox.focus();
    }

    #handleSearchUpdate = () => {
        if (this.#dirty === false) {
            this.#dirty = true;
            this.#elements.placeholder.style.display = 'none';
        }

        const isDropdownOpen = this.#elements.searchBox.getAttribute('aria-expanded') === 'true';
        if (isDropdownOpen) {
            this.debounce(async () => {
                this.data = await this.dataProvider.search(this.#fetchParameters);
                if (this.currentOptionIndex != null) {
                    this.#elements.options[this.currentOptionIndex].classList.remove('cb-dropdown-focused');
                    this.currentOptionIndex = null;
                }
                this.#render(this.#renderParameters);
                this.#elements.options = Array.from(this.#elements.tooltip.querySelectorAll('[role="option"]')).filter(option => {
                    return window.getComputedStyle(option).display !== 'none';
                });
            });
        }
    }

    /**
     * Retrieves data used by the render method.
     * @type {DataProvider<T>}
     */
    #_dataProvider = new InMemoryDataProvider([]);

    get dataProvider() {
        return this.#_dataProvider;
    }

    set dataProvider(dp) {
        if (dp instanceof InMemoryDataProvider) {
            this.debounce = createDebounce(100);
        } else {
            this.debounce = createDebounce(300);
        }
        this.#_dataProvider = dp;
    }

    /**
     * Function to render the dropdown options.
     *
     * @type {DropdownRenderFunction<T>}
     */
    render = (parameters) => {
        Array.from(parameters.tooltip.children).forEach(child => {
            if (parameters.data.findIndex(x => x.value === child.getAttribute("data-value")) === -1) {
                child.style.display = 'none';
            } else {
                child.style.display = 'block';
            }
        });

        const fragment = document.createDocumentFragment();

        parameters.data.forEach(x => {
            let child = Array.from(parameters.tooltip.children)
                .find(child => child.getAttribute("data-value") === x.value);

            if (child === undefined) {
                const li = document.createElement("li");
                li.setAttribute("role", "option");
                li.setAttribute("data-value", x.value);

                const text = document.createTextNode(x.display);
                li.appendChild(text);
                child = li
                fragment.appendChild(child);
            }

            if (parameters.selectedOptions.includes(x.value)) {
                child.classList.add('cb-dropdown-selected');
            } else {
                child.classList.remove('cb-dropdown-selected');
            }
        });

        parameters.tooltip.appendChild(fragment);
    };

    /**
     * @type {DropdownRenderFunction<T>}
     */
    #render = (parameters) => {
        this.render(parameters);
        this.#elements.options = Array.from(this.#elements.tooltip.querySelectorAll('[role="option"]')).filter(option => {
            return window.getComputedStyle(option).display !== 'none';
        });

        this.#selectedElements = this.#elements.options
            .filter(element => this.#selected.includes(element.getAttribute('data-value')));
    }

    /**
     * Creates a debounced version of a given function.
     *
     * @param {number} wait - The number of milliseconds to wait before invoking the debounced function.
     * @return {Function} - The debounced function.
     */
    debounce = createDebounce(300);


    /**
     * Updates the position of the dropdown tooltip.
     * @returns {void}
     */
    #update = () => {
        computePosition(this.#elements.button, this.#elements.tooltip, {
            placement: 'bottom',
            middleware: [offset(6), flip(), shift({padding: 5})],
        }).then(({x, y}) => {
            Object.assign(this.#elements.tooltip.style, {
                left: `${x}px`,
                top: `${y}px`,
                width: `${this.#elements.button.getBoundingClientRect().width}px`,
            });
        });
    }

    /**
     * Shows the dropdown.
     * @returns {void}
     */
    #showDropdown = async () => {
        this.previousPlaceholder = this.#elements.placeholder.textContent;

        document.addEventListener('pointerup', this.#attachHideDropdown);

        const opts = this.dataProvider.open(this.#fetchParameters);
        if (opts instanceof Promise) {
            this.#elements.tooltip.innerHTML = `
            <div style="display: flex;">
                <div class="spinner-border" role="status" style="margin-left: auto; margin-right: auto;"></div>
            </div>`

            opts.then(data => {
                this.data = data;

                if (this.currentOptionIndex != null) {
                    this.#elements.options[this.currentOptionIndex].classList.remove('cb-dropdown-focused');
                    this.currentOptionIndex = null;
                }
                this.#render(this.#renderParameters);

                if (this.#selectedElements.length === 1) {
                    const index = this.#elements.options.indexOf(this.#selectedElements[0]);
                    if (index !== -1) {
                        this.currentOptionIndex = this.#elements.options.indexOf(this.#selectedElements[0]);
                    }
                    this.#scrollToOption(this.#selectedElements[0]);
                }
            });
        } else {
            this.data = opts;

            if (this.currentOptionIndex != null) {
                this.#elements.options[this.currentOptionIndex].classList.remove('cb-dropdown-focused');
                this.currentOptionIndex = null;
            }
            this.#render(this.#renderParameters);

            if (this.#selectedElements.length === 1) {
                const index = this.#elements.options.indexOf(this.#selectedElements[0]);
                if (index !== -1) {
                    this.currentOptionIndex = this.#elements.options.indexOf(this.#selectedElements[0]);
                }
                this.#scrollToOption(this.#selectedElements[0]);
            }
        }

        this.#dirty = false;

        this.#elements.button.removeEventListener('pointerup', this.#showDropdown);
        this.#elements.searchBox.setAttribute('aria-expanded', 'true');

        this.#elements.tooltip.classList.add('cb-dropdown-active');

        this.#cleanup = autoUpdate(
            this.#elements.button,
            this.#elements.tooltip,
            this.#update,
        );
    }

    /**
     * Attaches an event listener to hide a dropdown on document pointerup, and removes itself after execution.
     *
     * @returns {void}
     */
    #attachHideDropdown = () => {
        document.removeEventListener('pointerup', this.#attachHideDropdown);
        document.addEventListener('pointerup', this.#hideDropdownEventHandler);
    }

    /**
     * Hides the dropdown.
     * @param {PointerEvent} ev - The MouseEvent triggering the hiding of the dropdown.
     * @returns {void}
     */
    #hideDropdownEventHandler = (ev) => {
        this.#hideDropdown(ev.composedPath().at(0));
    }

    /**
     * Hides the dropdown.
     * @param {Element|null} [target] - The target of the MouseEvent triggering the hiding of the dropdown.
     * @returns {void}
     */
    #hideDropdown = (target) => {
        let valueChanged = false;

        this.#elements.options.forEach(opt => opt.style.display = 'block');

        if (this.currentOptionIndex != null) {
            this.#elements.options[this.currentOptionIndex].classList.remove('cb-dropdown-focused');
            this.currentOptionIndex = null;
        }

        if (target != null && this.#isElementOrChildOf(target, this.#elements.tooltip) && target.role === 'option') {
            const selected = [target.getAttribute("data-value")];
            if (selected.length !== this.#selected.length ||
                selected.some((value, index) => value !== this.#selected[index])) {
                valueChanged = true;
            }

            this.#elements.placeholder.textContent = target.textContent;
            this.#selected = [target.getAttribute("data-value")];
            this.#selectedElements = [target];
        } else if (this.#selectedElements.length === 1) {
            this.#elements.placeholder.textContent = this.#selectedElements[0].textContent;
        } else {
            this.#elements.placeholder.textContent = this.previousPlaceholder;
        }

        this.#elements.searchBox.value = '';
        this.#elements.placeholder.style.display = 'block';

        this.#elements.tooltip.classList.remove('cb-dropdown-active');
        document.removeEventListener('pointerup', this.#hideDropdownEventHandler);

        this.#elements.button.addEventListener('pointerup', this.#showDropdown);
        this.#elements.searchBox.setAttribute('aria-expanded', 'false');

        this.#cleanup();
        this.#cleanup = null;

        if (valueChanged) {
            const ev = new CustomEvent("change", {
                detail: {
                    selected: this.#selected,
                }
            });

            this.dispatchEvent(ev);
        }
    }

    /**
     * Checks if an element is the same as a parent element or is a child of the parent element.
     * @param {Node} childElement - The child node to check.
     * @param {Node} parentElement - The parent node to check against.
     * @returns {boolean} Returns true if the child node is the same as the parent node or is a child of the parent node, otherwise returns false.
     */
    #isElementOrChildOf = (childElement, parentElement) => {
        if (parentElement === childElement) {
            return true;
        }

        while (childElement.parentNode) {
            childElement = childElement.parentNode;
            if (childElement === parentElement) {
                return true;
            }
        }

        return false;
    }

    /**
     * Focuses the next option in the dropdown
     */
    #moveFocusDown = () => {
        if (this.currentOptionIndex != null) {
            this.#elements.options[this.currentOptionIndex].classList.remove('cb-dropdown-focused');
        }

        if (this.currentOptionIndex != null) {
            this.currentOptionIndex = Math.min(this.currentOptionIndex + 1, this.#elements.options.length - 1)
        } else {
            this.currentOptionIndex = 0;
        }

        const currentOption = this.#elements.options[this.currentOptionIndex];
        currentOption.classList.add('cb-dropdown-focused');
        currentOption.focus();
        this.#scrollToOption(currentOption);
    };

    /**
     * Focuses the previous option in the dropdown
     */
    #moveFocusUp = () => {
        if (this.currentOptionIndex != null) {
            this.#elements.options[this.currentOptionIndex].classList.remove('cb-dropdown-focused');
        }

        if (this.currentOptionIndex != null) {
            this.currentOptionIndex = Math.max(this.currentOptionIndex - 1, 0);
        } else {
            this.currentOptionIndex = 0;
        }

        const currentOption = this.#elements.options[this.currentOptionIndex];
        currentOption.classList.add('cb-dropdown-focused');
        currentOption.focus();
        this.#scrollToOption(currentOption);
    };

    /**
     * Handles key press events for dropdown functionality.
     * @param {KeyboardEvent} event - The keyboard event object.
     */
    #handleKeyPress = (event) => {
        const {key} = event;

        const isDropdownOpen = this.#elements.searchBox.getAttribute('aria-expanded') === 'true';

        if (isDropdownOpen !== true) {
            this.#showDropdown();
        } else if (isDropdownOpen) {
            switch (key) {
                case 'Escape': {
                    event.preventDefault();
                    this.#hideDropdown();
                    break;
                }
                case 'ArrowDown': {
                    event.preventDefault();
                    this.#moveFocusDown();
                    break;
                }
                case 'ArrowUp': {
                    event.preventDefault();
                    this.#moveFocusUp();
                    break;
                }
                case 'Enter': {
                    event.preventDefault();
                    this.#hideDropdown(this.#elements.options[this.currentOptionIndex]);
                    break;
                }
            }
        }
    };


    /**
     * Scrolls the tooltip to an option item
     *
     * @private
     *
     * @param {HTMLElement} optionElement - The option item that the tooltip should scroll to.
     * @param {Object} [options] - An optional parameter object.
     * @param {'instant'|'smooth'} [options.scrollBehavior = 'instant'] - The scroll behavior. It could be 'instant' or 'smooth'.
     *
     * @returns {void}
     */
    #scrollToOption = (optionElement, {scrollBehavior = 'instant'} = {}) => {
        const offset = optionElement.offsetTop - (this.#elements.tooltip.offsetHeight / 2) + (optionElement.offsetHeight / 2);

        switch (scrollBehavior) {
            case "smooth":
                this.#elements.tooltip.style.scrollBehavior = 'smooth';
                break;
            case "instant":
            default:
                this.#elements.tooltip.style.scrollBehavior = 'auto';
        }

        this.#elements.tooltip.scrollTop = offset;
    };
}

/**
 * This function creates a debounced function that delays invoking `fn` until after `delay` milliseconds have elapsed
 * since the last time the debounced function was invoked.
 *
 * @param {number} delay - The number of milliseconds to delay
 * @returns {function(fn: () => void)} A new function that debounce the given function `fn`
 */
const createDebounce = (delay) => {
    let timeoutId;
    return (fn) => {
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => fn(), delay);
    };
}

if (!customElements.get('cb-dropdown')) {
    customElements.define('cb-dropdown', CbDropdown);
}
