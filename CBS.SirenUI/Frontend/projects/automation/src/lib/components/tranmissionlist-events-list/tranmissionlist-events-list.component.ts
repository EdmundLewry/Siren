import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatTable, MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';
import { TransmissionListEventEditDialog } from '../transmission-list-event-edit-dialog/transmission-list-event-edit-dialog.component';
import { TransmissionListEvent } from '../../interfaces/itransmission-list-event';
import { TransmissionListEventCreationData } from '../../interfaces/itransmission-list-event-creation-data';
import { RelativePosition, TransmissionListDetails } from '../../interfaces/interfaces';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { TransmissionListEventDetails } from '../../interfaces/itransmission-list-event-details';

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
      this.listId = this.route.snapshot.paramMap.get("itemId");
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
    this.dialog.open(TransmissionListEventEditDialog, {
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

        this.http.post<TransmissionListEvent>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events`, result).subscribe(response => {
        this.retrieveListInformation();
      }, error => console.error(error));
    });
  }
  
  
  public requestUpdateEvent(updatingEvent: TransmissionListEvent = null): void {
    var eventDetails = !updatingEvent ? null : this.retrieveEventById(updatingEvent.id);
    this.dialog.open(TransmissionListEventEditDialog, {
      width: '800px',
      data: eventDetails
    })
    .afterClosed()
      .subscribe((result: TransmissionListEventCreationData) => {
        if (result == null) return;

        console.log(result);

        this.http.put<TransmissionListEvent>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events/${updatingEvent.id}`, result).subscribe(response => {
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

  private retrieveEventById(id: number): TransmissionListEventDetails {
    var eventDetails = null;
    this.http.get<TransmissionListEventDetails>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events/${id}`, this.httpOptions)
      .subscribe(result => {
          eventDetails = result;
        }, 
        error => console.error(error)
      );
    return eventDetails;
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

  public getClearListTooltip(): string {
    return this.getListState() == "Playing" ? "Can't clear Playing list" : "Clear List";
  }

  dropTable(event: CdkDragDrop<TransmissionListEvent[]>) {
    
    let listEventId = this.dataSource.data[event.previousIndex].id;

    let request = {
      previousPosition: event.previousIndex,
      targetPosition: event.currentIndex
    };

    this.http.patch(`/proxy/api/1/automation/transmissionlist/${this.listId}/events/${listEventId}/move`, request).subscribe(result => {
      moveItemInArray(this.dataSource.data, event.previousIndex, event.currentIndex);
      this.table.renderRows();
      this.retrieveListInformation();
    }, error => console.error(error));
  }
}
