import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit, ViewChild } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatTable, MatTableDataSource } from '@angular/material/table';
import { Channel } from '../../interfaces/ichannel';
import { ChannelCreationData } from '../../interfaces/interfaces';
import { ChannelEditDialog } from '../channel-edit-dialog/channel-edit-dialog.component';

@Component({
  selector: 'lib-channel-list',
  templateUrl: './channel-list.component.html',
  styleUrls: ['./channel-list.component.css']
})
export class ChannelListComponent implements OnInit {
  public readonly dataSource: MatTableDataSource<Channel> = new MatTableDataSource();
  public readonly displayedColumns: string[] = [
    "id",
    "name",
    "listCount",
    "options"
  ];

  private readonly httpOptions = {
    headers: new HttpHeaders({ 'Content-Type': 'application/json' })
  };
  
  private readonly channelEndpoint = '/proxy/api/1/automation/channel';

  @ViewChild('table') table: MatTable<Channel>
  
  constructor(private http: HttpClient,
    public dialog: MatDialog) {
  }

  ngOnInit(): void {
    this.retrieveChannelInformation();
  }

  private retrieveChannelInformation(): void {
    this.http.get<Channel[]>(this.channelEndpoint, this.httpOptions).subscribe(result => {
      this.dataSource.data = result;
    }, error => console.error(error));
  }

  public requestAddNewChannel(): void {
    this.dialog.open(ChannelEditDialog, {
      width: '800px',
      data: null
    })
    .afterClosed()
    .subscribe((creationData: ChannelCreationData) => {
      if(creationData == null) return;

      this.http.post<Channel>(this.channelEndpoint, creationData).subscribe(response => {
        this.retrieveChannelInformation();
      },
      error => console.error(error));
    });
  }
}
