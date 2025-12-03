import {FetchingDataProvider} from "./dataProviders.js";

/**
 * Class representing a data provider for Clients.
 * @extends FetchingDataProvider
 */
export class ClientsDataProvider extends FetchingDataProvider {
    /**
     * Create a new ClientesDataProvider.
     * @param {string} allDisplayText - Localized display text for "ALL" clients dropdown option.
     */
    constructor(allDisplayText) {
        super(
            async ({page, pageSize, search}) => {
                const encodedSearch = encodeURIComponent(search);
                return await fetch(`/Cliente/Dropdown?page=${page}&pageSize=${pageSize}&search=${encodedSearch}`)
                    .then(r => r.json());
            },
            async (value) => {
                if (value === "") {
                    return {display: allDisplayText, value: ""};
                }
                
                const encodedValue = encodeURIComponent(value);
                return await fetch(`/Cliente/Dropdown?id=${encodedValue}`)
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