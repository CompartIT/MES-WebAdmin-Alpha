/**
 * The SelectionRange class is a simple CellRanges collection designed for easy manipulation of the multiple
 * consecutive and non-consecutive selections.
 *
 * @class SelectionRange
 * @util
 */
class SelectionRange {
  constructor(createCellRange) {
    /**
     * List of all CellRanges added to the class instance.
     *
     * @type {CellRange[]}
     */
    this.ranges = [];
    this.createCellRange = createCellRange;
  }

  /**
   * Check if selected range is empty.
   *
   * @returns {boolean}
   */
  isEmpty() {
    return this.size() === 0;
  }

  /**
   * Set coordinates to the class instance. It clears all previously added coordinates and push `coords`
   * to the collection.
   *
   * @param {CellCoords} coords The CellCoords instance with defined visual coordinates.
   * @returns {SelectionRange}
   */
  set(coords) {
    this.clear();
    this.ranges.push(this.createCellRange(coords));

    return this;
  }

  /**
   * Add coordinates to the class instance. The new coordinates are added to the end of the range collection.
   *
   * @param {CellCoords} coords The CellCoords instance with defined visual coordinates.
   * @returns {SelectionRange}
   */
  add(coords) {
    this.ranges.push(this.createCellRange(coords));

    return this;
  }

  /**
   * Removes from the stack the last added coordinates.
   *
   * @returns {SelectionRange}
   */
  pop() {
    this.ranges.pop();

    return this;
  }

  /**
   * Get last added coordinates from ranges, it returns a CellRange instance.
   *
   * @returns {CellRange|undefined}
   */
  current() {
    return this.peekByIndex(0);
  }

  /**
   * Get previously added coordinates from ranges, it returns a CellRange instance.
   *
   * @returns {CellRange|undefined}
   */
  previous() {
    return this.peekByIndex(-1);
  }

  /**
   * Returns `true` if coords is within selection coords. This method iterates through all selection layers to check if
   * the coords object is within selection range.
   *
   * @param {CellCoords} coords The CellCoords instance with defined visual coordinates.
   * @returns {boolean}
   */
  includes(coords) {
    return this.ranges.some(cellRange => cellRange.includes(coords));
  }

  /**
   * Clear collection.
   *
   * @returns {SelectionRange}
   */
  clear() {
    this.ranges.length = 0;

    return this;
  }

  /**
   * Get count of added all coordinates added to the selection.
   *
   * @returns {number}
   */
  size() {
    return this.ranges.length;
  }

  /**
   * Peek the coordinates based on the offset where that coordinate resides in the collection.
   *
   * @param {number} [offset=0] An offset where the coordinate will be retrieved from.
   * @returns {CellRange|undefined}
   */
  peekByIndex(offset = 0) {
    const rangeIndex = this.size() + offset - 1;
    let cellRange;

    if (rangeIndex >= 0) {
      cellRange = this.ranges[rangeIndex];
    }

    return cellRange;
  }

  [Symbol.iterator]() {
    return this.ranges[Symbol.iterator]();
  }
}

export default SelectionRange;
