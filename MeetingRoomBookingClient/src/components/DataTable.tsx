import React, { useState, useMemo } from 'react';

export interface Column<T> {
    header: string;
    accessor: keyof T | ((item: T) => React.ReactNode);
    sortable?: boolean;
    sortKey?: keyof T;
    align?: 'left' | 'center' | 'right';
    width?: string;
}

interface DataTableProps<T> {
    data: T[];
    columns: Column<T>[];
    pageSize?: number;
    searchPlaceholder?: string;
    emptyMessage?: string;
}

const DataTable = <T extends { id: string | number }>({
    data,
    columns,
    pageSize = 10,
    searchPlaceholder = 'Search...',
    emptyMessage = 'No data matching your search.'
}: DataTableProps<T>) => {
    const [searchTerm, setSearchTerm] = useState('');
    const [sortConfig, setSortConfig] = useState<{ key: keyof T | null; direction: 'asc' | 'desc' | null }>({
        key: null,
        direction: null,
    });
    const [currentPage, setCurrentPage] = useState(1);

    // Search Logic
    const filteredData = useMemo(() => {
        if (!searchTerm) return data;
        const lowerSearch = searchTerm.toLowerCase();

        return data.filter((item) => {
            return Object.values(item).some((val) => {
                if (val === null || val === undefined) return false;
                return String(val).toLowerCase().includes(lowerSearch);
            });
        });
    }, [data, searchTerm]);

    // Sort Logic
    const sortedData = useMemo(() => {
        if (!sortConfig.key || !sortConfig.direction) return filteredData;

        return [...filteredData].sort((a, b) => {
            const aVal = a[sortConfig.key!];
            const bVal = b[sortConfig.key!];

            if (aVal === null || aVal === undefined) return 1;
            if (bVal === null || bVal === undefined) return -1;

            if (aVal < bVal) return sortConfig.direction === 'asc' ? -1 : 1;
            if (aVal > bVal) return sortConfig.direction === 'asc' ? 1 : -1;
            return 0;
        });
    }, [filteredData, sortConfig]);

    // Pagination Logic
    const totalPages = Math.ceil(sortedData.length / pageSize);
    const paginatedData = useMemo(() => {
        const start = (currentPage - 1) * pageSize;
        return sortedData.slice(start, start + pageSize);
    }, [sortedData, currentPage, pageSize]);

    const handleSort = (column: Column<T>) => {
        if (!column.sortable) return;

        const key = (typeof column.accessor === 'string' ? column.accessor : column.sortKey) as keyof T;
        if (!key) return;

        let direction: 'asc' | 'desc' | null = 'asc';

        if (sortConfig.key === key) {
            if (sortConfig.direction === 'asc') direction = 'desc';
            else if (sortConfig.direction === 'desc') direction = null;
        }

        setSortConfig({ key: direction ? key : null, direction });
        setCurrentPage(1);
    };

    return (
        <div className="datatable-container">
            <div className="datatable-toolbar">
                <div className="search-bar" style={{ maxWidth: '320px', width: '100%' }}>
                    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                    </svg>
                    <input
                        type="text"
                        placeholder={searchPlaceholder}
                        value={searchTerm}
                        onChange={(e) => {
                            setSearchTerm(e.target.value);
                            setCurrentPage(1);
                        }}
                    />
                </div>
            </div>

            <div className="data-table-wrapper">
                <table className="data-table">
                    <thead>
                        <tr>
                            {columns.map((column, idx) => (
                                <th
                                    key={idx}
                                    onClick={() => handleSort(column)}
                                    className={column.sortable ? 'sortable' : ''}
                                    style={{
                                        textAlign: column.align || 'left',
                                        width: column.width,
                                        cursor: column.sortable ? 'pointer' : 'default'
                                    }}
                                >
                                    <div className="th-content" style={{ justifyContent: column.align === 'center' ? 'center' : column.align === 'right' ? 'flex-end' : 'flex-start' }}>
                                        {column.header}
                                        {column.sortable && (
                                            <span className={`sort-icon ${sortConfig.key === (typeof column.accessor === 'string' ? column.accessor : column.sortKey) ? 'active' : ''}`}>
                                                {sortConfig.key === (typeof column.accessor === 'string' ? column.accessor : column.sortKey) ? (
                                                    sortConfig.direction === 'asc' ? '↑' : '↓'
                                                ) : '↕'}
                                            </span>
                                        )}
                                    </div>
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {paginatedData.length === 0 ? (
                            <tr>
                                <td colSpan={columns.length} className="empty-row">
                                    {emptyMessage}
                                </td>
                            </tr>
                        ) : (
                            paginatedData.map((item) => (
                                <tr key={item.id}>
                                    {columns.map((column, idx) => (
                                        <td key={idx} style={{ textAlign: column.align || 'left' }}>
                                            {typeof column.accessor === 'function'
                                                ? column.accessor(item)
                                                : (item[column.accessor] as React.ReactNode)}
                                        </td>
                                    ))}
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            {totalPages > 1 && (
                <div className="datatable-pagination">
                    <div className="pagination-info">
                        Showing {Math.min((currentPage - 1) * pageSize + 1, sortedData.length)} to {Math.min(currentPage * pageSize, sortedData.length)} of {sortedData.length} entries
                    </div>
                    <div className="pagination-controls">
                        <button
                            disabled={currentPage === 1}
                            onClick={() => setCurrentPage(curr => curr - 1)}
                            className="pagination-btn"
                        >
                            Previous
                        </button>
                        <div className="pagination-pages">
                            <span className="current-page">Page {currentPage} of {totalPages}</span>
                        </div>
                        <button
                            disabled={currentPage === totalPages}
                            onClick={() => setCurrentPage(curr => curr + 1)}
                            className="pagination-btn"
                        >
                            Next
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default DataTable;
