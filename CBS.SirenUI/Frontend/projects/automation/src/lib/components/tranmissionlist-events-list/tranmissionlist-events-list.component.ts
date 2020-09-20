import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatTable, MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';
import { CreateEventDialogComponent } from '../create-event-dialog/create-event-dialog.component';
import { TransmissionListEvent } from '../../interfaces/itransmission-list-event';
import { TransmissionListEventCreationData } from '../../interfaces/itransmission-list-event-creation-data';
import { RelativePosition, TransmissionListDetails } from '../../interfaces/interfaces';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';

@Component({
  selector: 'lib-tranmissionlist-events-list',
  templateUrl: './tranmissionlist-events-list.component.html',
  styleUrls: ['./tranmissionlist-events-list.component.css']
})
export class TranmissionlistEventsListComponent implements OnInit {
  public readonly dataSource: MatTableDataSource<TransmissionListEvent> = new MatTableDataSource();
  public readonly displayedColumns: string[] = [
    "id",
    "eventState",
    "expectedDuration",
    "expectedStartTime",
    "actualStartTime",
    "actualEndTime",
    "options"
  ];

  private listId?: string;

  private readonly httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };
  
  //Store this enum as a public member so that we can reference it in the html
  public RelativePosition = RelativePosition;

  public transmissionList: TransmissionListDetails;
  @ViewChild('table') table: MatTable<TransmissionListEvent>

  constructor(private http: HttpClient,
              private route: ActivatedRoute,
              public dialog: MatDialog) {
    
  }

  public ngOnInit() {
    if (this.route.snapshot.paramMap.has("itemId")) {
      this.listId = this.route.snapshot.paramMap.get("itemId") as string;
    }

    this.retrieveListInformation();
  }

  public requestDeleteConfirmation(listEvent: TransmissionListEvent): void {
    this.openConfirmationDialog("Delete event?").afterClosed().subscribe(confirmed => {
      if (!confirmed) return;

      this.http.delete(`/proxy/api/1/automation/transmissionlist/${this.listId}/events/${listEvent.id}`, this.httpOptions).subscribe(result => {
        this.retrieveListInformation();
      }, error => console.error(error));
    });

  }

  private openConfirmationDialog(dialogText: string) {
    return this.dialog.open(ConfirmationDialogComponent, {
      width: '300px',
      data: { title: dialogText }
    });
  }

  public requestListPlay(): void {
    this.http.post(`/proxy/api/1/automation/transmissionlist/${this.listId}/play`, this.httpOptions).subscribe(result => {
      this.retrieveListInformation();
    }, error => console.error(error));
  }

  public requestListStop(): void {
    console.error("Stop not currently supported");
  }

  public requestAddNewEvent(relativeEvent: TransmissionListEvent = null, relativePosition: RelativePosition = null): void {
    this.dialog.open(CreateEventDialogComponent, {
      width: '800px',
      data: {}
    })
    .afterClosed()
      .subscribe((result: TransmissionListEventCreationData) => {
      if (result == null) return;
      
      if(relativeEvent != null && relativePosition != null)
      {
        result.listPosition = {
          associatedEventId: relativeEvent.id,
          relativePosition: relativePosition
        };
      }

      console.log(result);

        this.http.post<TransmissionListEvent>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events`, result).subscribe(result => {
        this.retrieveListInformation();
      }, error => console.error(error));
    });
  }

  public requestClearList(): void {
    this.openConfirmationDialog("Clear all events from the list?").afterClosed().subscribe(confirmed => {
      if (!confirmed) return;

      this.http.post(`/proxy/api/1/automation/transmissionlist/${this.listId}/clear`, this.httpOptions).subscribe(result => {
        this.retrieveListInformation();
      }, error => console.error(error));
    });
  }

  private retrieveEvents(): void {
    this.http.get<TransmissionListEvent[]>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events`, this.httpOptions).subscribe(result => {
      this.dataSource.data = result;
    }, error => console.error(error));
  }
  
  private retrieveListInformation(): void {
    this.http.get<TransmissionListDetails>(`/proxy/api/1/automation/transmissionlist/${this.listId}`, this.httpOptions).subscribe(result => {
      this.transmissionList = result;
      this.dataSource.data = this.transmissionList.events;
    }, error => console.error(error));
  }

  public getListState(): string {
    return this.transmissionList?.listState ?? "";
  }

  dropTable(event: CdkDragDrop<TransmissionListEvent[]>) {
    moveItemInArray(this.dataSource.data, event.previousIndex, event.currentIndex);
    this.table.renderRows();
  }
}
