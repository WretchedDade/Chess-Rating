const faSize = 'fa fa-2x';
const faSquare = `${faSize} fa-square`;
const faCheckSquare = `${faSize} fa-check-square`;

class SheetsTable {
    constructor(range, sheet, tableSelector, headings, refreshFunction) {
        this.Range = range;
        this.Sheet = sheet;
        this.TableSelector = tableSelector;
        this.THeadSelector = `${this.TableSelector} thead`;
        this.TBodySelector = `${this.TableSelector} tbody`;
        this.RefreshFunction = refreshFunction;

        if (headings) {
            this.Headings = headings;
            this.HeadingsDefinedByUser = true;
        } else {
            this.HeadingsDefinedByUser = false;
        }

        this.Refresh();
    }

    Refresh() {
        var parent = this;
        this.RefreshValues().then(function (response) {
            parent.Values = response.result.values;

            if (!parent.Values || parent.Values.length == 0) {
                //TODO - Make this more robust. Modal?
                console.warn(`No data found for range ${parent.Range}`);
                alert(`No data found for range ${parent.Range}`);
                return;
            }

            parent.RefreshTable().then(parent.RefreshFunction);
        });
    }

    RefreshValues() {
        var request = {
            spreadsheetId: this.Sheet.SpreadsheetID,
            range: this.Range
        }

        return this.Sheet.Get(request);
    }

    RefreshTable() {
        return new Promise((resolve, reject) => {
            this.Clear();

            var startingIndex = this.HeadingsDefinedByUser ? 0 : 1;

            if (this.HeadingsDefinedByUser)
                this.AppendHeaderRow(this.Headings);
            else
                this.AppendHeaderRow(this.Values[0]);

            var row;
            for (var i = startingIndex; i < this.Values.length; i++) {
                row = this.Values[i];

                if (row[0] != '')
                    this.AppendRow(row);
                else {
                    this.Values.splice(i, 1);
                    i--;
                }
            }

            resolve();
        });
    }

    Clear() {
        $(this.THeadSelector).empty();
        $(this.TBodySelector).empty();
    }

    AppendHeaderRow(headerRow) {
        var ths = '';
        for (var i = 0; i < headerRow.length; i++)
            ths += `<th class="text-center">${headerRow[i]}</th>`;

        this.AppendTableRow(ths, this.THeadSelector);
    }

    AppendRow(row) {

        var value;
        var tds = '';

        for (var i = 0; i < row.length; i++) {
            value = row[i].toLowerCase();

            if (value === 'true' || value === 'false')
                tds += `<td class="text-center text-primary align-middle"><i class="${value == 'true' ? faCheckSquare : faSquare}"></i></td>`;
            else if (isNaN(value) && !value.includes('%'))
                tds += `<td class="align-middle">${row[i]}</td>`;
            else
                tds += `<td class="text-center align-middle">${row[i]}</td>`;
        }

        this.AppendTableRow(tds, this.TBodySelector);
    }

    AppendTableRow(content, selector) {
        $(selector).append(`<tr>${content}</tr>`);
    }

    AddRow(columns) {
        return new Promise((resolve, reject) => {
            this.Values.push(columns);

            var valueRangeBody = {
                values: this.Values
            };

            var parent = this;
            this.Sheet.Put(this.Range, valueRangeBody).then(function (response) {
                parent.SendGetRequestToAppScriptUrl();
                parent.Refresh();
            });

            resolve();
        });
    }

    SendGetRequestToAppScriptUrl() {
        return $.ajax({
            url: this.Sheet.AppScriptUrl,
            type: 'GET',
            dataType: 'jsonp'
        });
    }
}