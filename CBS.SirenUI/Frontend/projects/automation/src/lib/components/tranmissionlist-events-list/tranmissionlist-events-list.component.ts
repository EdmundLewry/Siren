import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { MatTableDataSource } from '@angular/material/table';
import { ActivatedRoute } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';
import { CreateEventDialogComponent } from '../create-event-dialog/create-event-dialog.component';
import { TransmissionList } from '../../interfaces/itransmission-list';
import { TransmissionListEvent } from '../../interfaces/itransmission-list-event';
import { TransmissionListEventCreationData } from '../../interfaces/itransmission-list-event-creation-data';

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
    "options"
  ];

  private listId?: string;

  private readonly fakeData: TransmissionListEvent[] = [
    { id: 1, eventState: "Scheduled", expectedDuration: "00:00:30:00", expectedStartTime: "2020-03-22T00:00:10:05" },
    { id: 2, eventState: "Running", expectedDuration: "00:00:30:00", expectedStartTime: "2020-03-22T00:00:40:05" },
  ];

  private readonly httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };

  private transmissionList: TransmissionList;

  constructor(private http: HttpClient,
              private route: ActivatedRoute,
              public dialog: MatDialog) {
    this.dataSource.data = this.fakeData;
  }

  public ngOnInit() {
    if (this.route.snapshot.paramMap.has("itemId")) {
      this.listId = this.route.snapshot.paramMap.get("itemId") as string;
    }

    this.retrieveEvents();
  }

  public requestDeleteConfirmation(listEvent: TransmissionListEvent): void {
    this.openConfirmationDialog("Delete event?").afterClosed().subscribe(confirmed => {
      if (!confirmed) return;

      this.http.delete(`/proxy/api/1/automation/transmissionlist/${this.listId}/events/${listEvent.id}`, this.httpOptions).subscribe(result => {
        this.retrieveEvents();
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
      this.retrieveEvents();
    }, error => console.error(error));
  }

  public requestListStop(): void {
    console.error("Stop not currently supported");
  }

  public requestAddNewEvent(): void {
    this.dialog.open(CreateEventDialogComponent, {
      width: '800px',
      data: {}
    })
    .afterClosed()
      .subscribe((result: TransmissionListEventCreationData) => {
      if (result == null) return;

      console.log(result);

        this.http.post<TransmissionListEvent>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events`, result).subscribe(result => {
        this.retrieveEvents();
      }, error => console.error(error));
    });
  }

  public requestClearList(): void {
    this.openConfirmationDialog("Clear all events from the list?").afterClosed().subscribe(confirmed => {
      if (!confirmed) return;

      this.http.post(`/proxy/api/1/automation/transmissionlist/${this.listId}/clear`, this.httpOptions).subscribe(result => {
        this.retrieveEvents();
      }, error => console.error(error));
    });
  }

  private retrieveEvents(): void {
    this.http.get<TransmissionListEvent[]>(`/proxy/api/1/automation/transmissionlist/${this.listId}/events`, this.httpOptions).subscribe(result => {
      this.dataSource.data = result;
    }, error => console.error(error));
  }
}
