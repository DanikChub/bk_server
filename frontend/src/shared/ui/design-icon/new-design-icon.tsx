/* eslint-disable @next/next/no-img-element */
export function NewDesignIcon({file,className=''}:{file:string;className?:string}){
  return <img src={'/design/new/'+file} className={`new-design-icon ${className}`} alt="" aria-hidden="true"/>;
}
