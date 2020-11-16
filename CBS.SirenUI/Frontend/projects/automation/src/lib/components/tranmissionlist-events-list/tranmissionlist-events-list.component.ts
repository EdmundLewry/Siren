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
import { Observable } from 'rxjs';

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
  
  public requestListNext(): void {
    this.http.post(`/proxy/api/1/automation/transmissionlist/${this.listId}/next`, this.httpOptions).subscribe(result => {
      this.retrieveListInformation();
    }, error => console.error(error));
  }

  public requestListStop(): void {
    this.http.post(`/proxy/api/1/automation/transmissionlist/${this.listId}/stop`, this.httpOptions).subscribe(result => {
      this.retrieveListInformation();
    }, error => console.error(error));
  }

  public requestAddNewEvent(relativeEvent: TransmissionListEvent = null, relativePosition: RelativePosition = null): void {
    this.dialog.open(TransmissionListEventEditDialog, {
      width: '800px',
      data: null
    })
    .afterClosed()
    .subscribe((creationData: TransmissionListEventCreationData) => {
      if (creationData == null) return;
      
      if(relativeEvent != null && relativePosition != null)
      {
        creationData.listPosition = {
          associatedEventId: relativeEvent.id,
          relativePosition: relativePosition
        };
      }

      console.log(creationData);

      this.http.post<TransmissionListEvent>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events`, creationData).subscribe(response => {
        this.retrieveListInformation();
      }, error => console.error(error));
    });
  }
  
  
  public requestUpdateEvent(updatingEvent: TransmissionListEvent = null): void {
    var eventDetails = null;
    if(!updatingEvent)
    {
      console.error("Received a request to update an event, but no event was passed");
      return;
    }

    this.retrieveEventById(updatingEvent.id).subscribe(updatingEventDetails => {
      if (updatingEventDetails == null) return;

      this.dialog.open(TransmissionListEventEditDialog, {
        width: '800px',
        data: updatingEventDetails
      })
      .afterClosed()
      .subscribe((creationData: TransmissionListEventCreationData) => {
        if (creationData == null) return;

        console.log("Creation Data: ", creationData);
        
        this.http.put<TransmissionListEvent>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events/${updatingEvent.id}`, creationData).subscribe(response => {
          this.retrieveListInformation();
        }, error => console.error(error));
      });
    }, error => console.log(error));
  }

  public requestClearList(): void {
    this.openConfirmationDialog("Clear all events from the list?").afterClosed().subscribe(confirmed => {
      if (!confirmed) return;

      this.http.post(`/proxy/api/1/automation/transmissionlist/${this.listId}/clear`, this.httpOptions).subscribe(result => {
        this.retrieveListInformation();
      }, error => console.error(error));
    });
  }

  private retrieveEventById(id: number): Observable<TransmissionListEventDetails> {
    return this.http.get<TransmissionListEventDetails>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events/${id}`, this.httpOptions);
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

  public dropTable(event: CdkDragDrop<TransmissionListEvent[]>) {
    
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
