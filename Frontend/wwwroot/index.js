const { ajax, defer, fromEvent, interval, filter, map, catchError, of } = rxjs;


const getSockets = () => {
    const url = '/api/Frontend/Sockets'
    return ajax.ajax.getJSON(url).pipe(
        map(response => response),
        catchError(error => {
            console.error('Error: ', error);
            return of(error);
        })
    );
};


function closeSocket(id) {
    const url = `/api/Frontend/CloseSocket/${id()}`
    ajax.ajax.put(url).pipe(
        map(response => response),
        catchError(error => {
            console.error('Error: ', error);
            return of(error);
        })
    ).subscribe({
        next: data => {
            console.log(data);
            
        },
        error: err => console.error('Error: ', err),
        complete: () => console.log('Request complete')
    });

}


var viewModel = {};

getSockets().subscribe({
    next: data => {
        viewModel.sockets = ko.mapping.fromJS(data);
        ko.applyBindings(viewModel);
    },
    error: err => console.error('Error: ', err),
    complete: () => console.log('Request complete')
});

