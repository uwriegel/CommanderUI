import { Component, NgZone } from '@angular/core'
import { IColumnSortEvent } from '../../columns/columns.component'
import { Columns } from 'src/app/model/model'
import { ICommanderView, IProcessor } from 'src/app/interfaces/commander-view'

@Component({
    selector: 'app-test-columns',
    templateUrl: './columns.component.html',
    styleUrls: ['./columns.component.css']
})
export class TestColumnsComponent implements ICommanderView {

    constructor(private zone: NgZone) { 
        commanderViewLeft = this
        CommanderLeft.ready()
    }

    columns: Columns

    setColumns(columns: Columns) {
        console.log("New Columns", columns)
        this.zone.run(() => this.columns = columns)
    }
    
    onSort(sortEvent: IColumnSortEvent) {
        console.log(`Sorting: ${sortEvent.index} ascending: ${sortEvent.ascending}`)
    }

    onChange(path: string) {
        // TODO: OnCHange
//        this.response = from(this.connection.get(callerId, path, path))
    }
}

declare var CommanderLeft : IProcessor
declare var commanderViewLeft : ICommanderView