import {FetchingDataProvider} from "./dataProviders.js";

/**
 * Class representing a data provider for Clients.
 * @extends FetchingDataProvider
 */
export class ClientsDataProvider extends FetchingDataProvider {
    /**
     * Create a new ClientesDataProvider.
     * @param {string}  urlBase  - URL REQUEST.
     * @param {string} allDisplayText - Localized display text for "ALL" clients dropdown option.

     */
    constructor(urlBase, allDisplayText, ) {
        super(
            async ({page, pageSize, search}) => {
                const encodedSearch = encodeURIComponent(search);
                return await fetch(`${urlBase}?page=${page}&pageSize=${pageSize}&search=${encodedSearch}`)
                    .then(r => r.json());
            },
            async (value) => {
                if (value === "") {
                    return {display: allDisplayText, value: ""};
                }
                
                const encodedValue = encodeURIComponent(value);
                return await fetch(`${urlBase}?id=${encodedValue}`)
                    .then(r => r.json())
                    .then(j => j?.data?.at(0))
                    .catch(() => undefined);
            }
        );

        this.transformData = ((x, _) => {
            return [{display: allDisplayText, value: ""}, ...x];
        })
    }
}