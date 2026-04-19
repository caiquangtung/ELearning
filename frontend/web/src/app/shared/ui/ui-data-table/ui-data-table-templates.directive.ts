import { Directive, TemplateRef } from '@angular/core';

@Directive({
  selector: 'ng-template[uiDataTableHeader]',
  standalone: true,
})
export class UiDataTableHeaderTemplateDirective {
  constructor(readonly templateRef: TemplateRef<unknown>) {}
}

@Directive({
  selector: 'ng-template[uiDataTableBody]',
  standalone: true,
})
export class UiDataTableBodyTemplateDirective {
  constructor(readonly templateRef: TemplateRef<unknown>) {}
}

@Directive({
  selector: 'ng-template[uiDataTableEmpty]',
  standalone: true,
})
export class UiDataTableEmptyTemplateDirective {
  constructor(readonly templateRef: TemplateRef<unknown>) {}
}

