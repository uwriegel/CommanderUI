import { Component, OnInit, Input, HostBinding, AfterViewInit, ElementRef, ViewChild } from '@angular/core'

@Component({
    selector: 'app-grid-splitter',
    templateUrl: './grid-splitter.component.html',
    styleUrls: ['./grid-splitter.component.css']
})
export class GridSplitterComponent implements OnInit, AfterViewInit {

    @ViewChild("splitter") splitter: ElementRef
    @Input() isVertical
    @Input() 
    get isLastVisible() { return this._isLastVisible }
    set isLastVisible(value: boolean) { 
        this._isLastVisible = value
        if (this.view2) {
            this.view2.hidden = !value
            if (this.view2.hidden) {
                this.hiddenGridValue = this.view1.style.flex
                this.view1.style.flex = ""
            }
            else
                this.view1.style.flex = this.hiddenGridValue
        }

    }
    private _isLastVisible = true
    constructor(private elRef: ElementRef) { }

    ngOnInit() { }

    ngAfterViewInit() {
        this.view1 = this.elRef.nativeElement.getElementsByClassName("first")[0]
        this.view2 = this.elRef.nativeElement.getElementsByClassName("last")[0]
        if (this.view2)
            this.view2.hidden = !this.isLastVisible
    }

    private onSplitterMouseDown(evt: MouseEvent) {
        if (evt.which != 1)
            return
        
        const size1 = this.isVertical ? this.view1.offsetHeight : this.view1.offsetWidth
        const size2 = this.isVertical ? this.view2.offsetHeight : this.view2.offsetWidth
        const initialPosition = this.isVertical ? evt.pageY : evt.pageX

        const onmousemove = (evt: MouseEvent) => {
            let delta = (this.isVertical ? evt.pageY : evt.pageX) - initialPosition
            if (delta < 10 - size1)
                delta = 10 - size1
            if (delta > (this.isVertical ? this.view1.parentElement.offsetHeight : this.view1.parentElement.offsetWidth) - 10 - size1 - 6)
                delta = (this.isVertical ? this.view1.parentElement.offsetHeight : this.view1.parentElement.offsetWidth) - 10 - size1 - 6

            const newSize1 = size1 + delta
            const newSize2 = size2 - delta

            const procent1 = newSize1 / (newSize1 + newSize2 + 
                (this.isVertical ? this.splitter.nativeElement.offsetHeight : this.splitter.nativeElement.offsetWidth)) * 100
            this.view1.style.flex = `0 0 ${procent1}%`
            this.view2.style.flexGrow = `1`
            //this.onChanged()

            evt.stopPropagation()
            evt.preventDefault()
        }

        const onmouseup = (evt: MouseEvent) => {
            window.removeEventListener('mousemove', onmousemove, true)
            window.removeEventListener('mouseup', onmouseup, true)

            evt.stopPropagation();
            evt.preventDefault();
        }

        window.addEventListener('mousemove', onmousemove, true)
        window.addEventListener('mouseup', onmouseup, true)

        evt.stopPropagation()
        evt.preventDefault()        

    }

    private view1: HTMLElement
    private view2: HTMLElement
    private hiddenGridValue
}
