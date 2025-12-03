export class FetchingDataProvider {
    /**
     * Constructor function for loading data.
     *
     * @param {function({page: number, pageSize: number, search: string}): Promise<{moreToLoad: boolean, data: DropdownOption[]}>} loadData -
     * The function that loads the data from a specified source.
     * @param {function(string): Promise<DropdownOption>} find -
     * The function that finds an option from a value.
     * @param {number} [pageSize=30] - The number of items to fetch per page. Default value is 30.
     * @constructor
     *
     */
    constructor(loadData, find, pageSize = 30) {
        this.loadData = loadData;
        this.find = find;
        this.#pageSize = pageSize;
    }

    #pageSize = 0;

    /**
     *
     * @type {Object.<string, {moreToLoad: boolean, page: number, cache: DropdownOption[]}>}
     */
    cache = {};

    /**
     * @param {DropdownOption[]} data
     * @param {DropdownFetchParameters|null} parameters
     * @returns {DropdownOption[]}
     */
    transformData = (data, parameters) => data;

    /**
     * Fetches data asynchronously based on the given search parameter.
     *
     * @param {DropdownFetchParameters} parameters
     * @returns {Promise<DropdownOption[]>} - A promise that resolves with the fetched data array.
     */
    #fetch = async (parameters) => {
        const {search} = parameters;

        if (!this.cache[search]) {
            this.cache[search] = {
                page: 0,
                cache: [],
                moreToLoad: true
            };
        }

        const searchCache = this.cache[search];
        this.moreToLoad = searchCache.moreToLoad;

        const data = await this.loadData({page: searchCache.page, pageSize: this.#pageSize, search}).then(data => {
            searchCache.page += 1;

            if (data.moreToLoad === false) {
                searchCache.moreToLoad = false;
                this.moreToLoad = false;
            }

            searchCache.cache = searchCache.cache.concat(data.data);

            return searchCache.cache;
        })
            .catch(err => {
                console.error(err);
                return searchCache.cache;
            });

        return this.transformData(data, parameters);
    };

    open = async (params) => {
        if (!this.cache[params.search]) {
            this.cache[params.search] = {
                page: 0,
                cache: [],
                moreToLoad: true
            };
        }

        const searchCache = this.cache[params.search];
        this.moreToLoad = searchCache.moreToLoad;

        if (searchCache.cache.length === 0 && this.moreToLoad) {
            return await this.#fetch(params);
        } else {
            return this.transformData(searchCache.cache, null);
        }
    }

    search = async (params) => {
        return await this.#fetch(params);
    };

    scroll = async (params) => {
        return await this.#fetch(params);
    };

    moreToLoad = true;
}

export class InMemoryDataProvider {
    /**
     * Constructor function for loading data.
     *
     * @param {DropdownOption[]} data - The function that loads the
     * data from a specified source.
     * @constructor
     *
     */
    constructor(data) {
        this.data = data;
        this.moreToLoad = false;
    }

    /**
     * Returns data from the provided array.
     *
     * @param {DropdownFetchParameters} options - The options object.
     * @returns {DropdownOption[]} - A promise that resolves with the fetched data array.
     */
    #fetch = ({search}) => {
        return this.filter(this.data, search);
    }

    /**
     * Filters an array based on a normalized, case-insensitive search string.
     *
     * Used for filtering the results when using static data.
     * It normalizes Unicode characters  in input values, removes accents, and ignores case during the comparison,
     *
     * @param {DropdownOption[]} array - The array to be filtered.
     * @param {string} searchString - The search string used for filtering.
     * @returns {DropdownOption[]} - The filtered array.
     */
    filter = (array, searchString) => {
        if (searchString == null || searchString.trim().length === 0) {
            return array;
        }

        const normalizedSearchString = searchString.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();

        return array.filter(item => {
            const normalizedItem = item.display.normalize("NFD").replace(/[\u0300-\u036f]/g, "").toLowerCase();
            return normalizedItem.includes(normalizedSearchString);
        });
    }

    open = (params) => {
        return this.#fetch(params);
    }

    search = (params) => {
        return this.#fetch(params);
    };

    scroll = (params) => {
        return this.#fetch(params);
    };

    find = (value) => {
        return this.data.find(x => x.value === value);
    }

    moreToLoad = true;
}
