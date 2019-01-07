class SheetsTable {
    constructor(range, sheet, tableSelector, headings) {
        this.Range = range;
        this.Sheet = sheet;
        this.TableSelector = tableSelector;
        this.THeadSelector = `${this.TableSelector} thead`;
        this.TBodySelector = `${this.TableSelector} tbody`;

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

            parent.RefreshTable();
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

    AppendHeaderRow(headerRow) {
        var ths = '';
        for (var i = 0; i < headerRow.length; i++)
            ths += `<th>${headerRow[i]}</th>`;

        this.AppendTableRow(ths, this.THeadSelector);
    }

    AppendRow(row) {
        var tds = '';
        for (var i = 0; i < row.length; i++)
            tds += `<td>${row[i]}</td>`;

        this.AppendTableRow(tds, this.TBodySelector);
    }

    AppendTableRow(content, selector) {
        $(selector).append(`<tr>${content}</tr>`);
    }

    AddRow(columns) {
        this.Values.push(columns);

        var valueRangeBody = {
            values: this.Values
        };

        var parent = this;
        this.Sheet.Put(this.Range, valueRangeBody).then(function (response) {
            parent.Sheet.GetAppScript();
            parent.Refresh();
        });
    }
}