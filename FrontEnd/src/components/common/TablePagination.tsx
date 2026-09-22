import { useState } from 'react';
import { Pagination, Select } from 'antd';

interface TablePaginationProps {
  totalItem?: number;
  pageSize?: number;
  pageNumber?: number;
  pageSizeOptions?: number[];
  onChangeFunction: (page: number, pageSize: number) => void;
  loading?: boolean;
  entryName?: string;
}

export default function TablePagination({
  totalItem = 0, pageSize = 10, pageNumber = 1,
  pageSizeOptions = [10, 20, 50, 100],
  onChangeFunction, loading = false, entryName = 'records',
}: TablePaginationProps) {
  const [currentPageSize, setCurrentPageSize] = useState(pageSize);
  const startItem = totalItem === 0 ? 0 : (pageNumber - 1) * currentPageSize + 1;
  const endItem = Math.min(pageNumber * currentPageSize, totalItem);

  const handlePageSizeChange = (value: number) => {
    setCurrentPageSize(value);
    onChangeFunction(pageNumber, value);
  };

  const options = pageSizeOptions.map((o) => ({ value: o, label: `${o} rows / page` }));

  return (
    <div style={{
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      width: '100%', padding: '1rem 0',
    }}>
      <div style={{ flex: 1, paddingLeft: '0.4rem' }}>
        {`${startItem}-${endItem} of ${totalItem} ${entryName}`}
      </div>
      <div style={{ flex: 1, display: 'flex', justifyContent: 'center' }}>
        <Pagination
          current={pageNumber} total={totalItem} pageSize={currentPageSize}
          onChange={onChangeFunction} disabled={loading} showSizeChanger={false}
        />
      </div>
      <div style={{ flex: 1, display: 'flex', justifyContent: 'flex-end' }}>
        <Select value={currentPageSize} style={{ width: '11rem' }}
          options={options} onChange={handlePageSizeChange} />
      </div>
    </div>
  );
}